using System.Net;
using System.Text.Json;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Europcar.Rental.Api.Middleware;

/// <summary>
/// Middleware global de manejo de excepciones.
/// Garantiza que cualquier error producido en el pipeline se serialice como un
/// <see cref="ApiResponse{T}"/> uniforme, con TraceId, status HTTP correcto y
/// nivel de detalle controlado por entorno (dev vs prod).
/// </summary>
public class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Cliente abortó la conexión. No vale la pena loguearlo como error.
            _logger.LogDebug("Request abortada por el cliente: {Path}", context.Request.Path);
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 499; // Convención NGINX: Client Closed Request
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            // No podemos modificar la respuesta una vez enviada.
            _logger.LogError(exception, "Excepción después de iniciar la respuesta. Path={Path}", context.Request.Path);
            return;
        }

        var traceId = context.TraceIdentifier;
        var response = context.Response;
        response.Clear();
        response.ContentType = "application/json; charset=utf-8";

        var payload = new ApiResponse<object>
        {
            Success = false,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ValidationException validationEx:
                payload.StatusCode = validationEx.StatusCode;
                payload.Message = validationEx.Message;
                payload.Errors = validationEx.Errors;
                _logger.LogInformation(
                    "Validación fallida [{TraceId}] en {Path}: {Errors}",
                    traceId, context.Request.Path,
                    string.Join("; ", validationEx.Errors.SelectMany(kv => kv.Value.Select(v => $"{kv.Key}: {v}"))));
                break;

            case BusinessException businessEx:
                payload.StatusCode = businessEx.StatusCode;
                payload.Message = businessEx.Message;
                _logger.LogWarning(
                    "BusinessException [{TraceId}] {Status} en {Path}: {Message}",
                    traceId, businessEx.StatusCode, context.Request.Path, businessEx.Message);
                break;

            case UnauthorizedAccessException uaEx:
                payload.StatusCode = (int)HttpStatusCode.Unauthorized;
                payload.Message = "No autorizado";
                _logger.LogWarning("UnauthorizedAccess [{TraceId}] en {Path}: {Message}", traceId, context.Request.Path, uaEx.Message);
                break;

            case KeyNotFoundException knfEx:
                payload.StatusCode = (int)HttpStatusCode.NotFound;
                payload.Message = string.IsNullOrWhiteSpace(knfEx.Message) ? "Recurso no encontrado" : knfEx.Message;
                _logger.LogInformation("KeyNotFound [{TraceId}] en {Path}: {Message}", traceId, context.Request.Path, knfEx.Message);
                break;

            case ArgumentException argEx:
                payload.StatusCode = (int)HttpStatusCode.BadRequest;
                payload.Message = argEx.Message;
                _logger.LogWarning("ArgumentException [{TraceId}] en {Path}: {Message}", traceId, context.Request.Path, argEx.Message);
                break;

            case TimeoutException:
            case TaskCanceledException:
                payload.StatusCode = (int)HttpStatusCode.GatewayTimeout;
                payload.Message = "La operación tardó demasiado en completarse. Intenta nuevamente.";
                _logger.LogError(exception, "Timeout [{TraceId}] en {Path}", traceId, context.Request.Path);
                break;

            case DbUpdateConcurrencyException:
                payload.StatusCode = (int)HttpStatusCode.Conflict;
                payload.Message = "El registro fue modificado por otro usuario. Recarga e intenta de nuevo.";
                _logger.LogWarning("Concurrency conflict [{TraceId}] en {Path}", traceId, context.Request.Path);
                break;

            case DbUpdateException dbEx:
                MapDbUpdateException(dbEx, payload, traceId, context.Request.Path);
                break;

            case PostgresException pgEx:
                MapPostgresException(pgEx, payload, traceId, context.Request.Path);
                break;

            case NpgsqlException npgEx:
                payload.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                payload.Message = "La base de datos no está disponible en este momento. Intenta más tarde.";
                _logger.LogError(npgEx, "NpgsqlException [{TraceId}] en {Path}", traceId, context.Request.Path);
                break;

            case JsonException jsonEx:
                payload.StatusCode = (int)HttpStatusCode.BadRequest;
                payload.Message = "El cuerpo de la petición no es un JSON válido.";
                _logger.LogWarning(jsonEx, "JSON inválido [{TraceId}] en {Path}", traceId, context.Request.Path);
                break;

            case NotImplementedException:
                payload.StatusCode = (int)HttpStatusCode.NotImplemented;
                payload.Message = "Funcionalidad no implementada.";
                _logger.LogError(exception, "NotImplemented [{TraceId}] en {Path}", traceId, context.Request.Path);
                break;

            default:
                payload.StatusCode = (int)HttpStatusCode.InternalServerError;
                payload.Message = "Ha ocurrido un error inesperado. Si persiste, contacta a soporte indicando el TraceId.";
                _logger.LogError(exception, "Excepción no controlada [{TraceId}] en {Path}", traceId, context.Request.Path);
                break;
        }

        // Solo en desarrollo exponemos el detalle técnico.
        if (_env.IsDevelopment())
        {
            payload.Detail = BuildDetail(exception);
        }

        response.StatusCode = payload.StatusCode == 0 ? 500 : payload.StatusCode;
        await response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static void MapDbUpdateException(DbUpdateException dbEx, ApiResponse<object> payload, string traceId, PathString path)
    {
        if (dbEx.InnerException is PostgresException innerPg)
        {
            MapPostgresException(innerPg, payload, traceId, path);
            return;
        }

        payload.StatusCode = (int)HttpStatusCode.Conflict;
        payload.Message = "No fue posible guardar los cambios en la base de datos.";
    }

    private static void MapPostgresException(PostgresException pg, ApiResponse<object> payload, string traceId, PathString path)
    {
        // Códigos PostgreSQL más comunes: https://www.postgresql.org/docs/current/errcodes-appendix.html
        switch (pg.SqlState)
        {
            case "23505": // unique_violation
                payload.StatusCode = (int)HttpStatusCode.Conflict;
                payload.Message = "Ya existe un registro con esos datos únicos.";
                break;
            case "23503": // foreign_key_violation
                payload.StatusCode = (int)HttpStatusCode.Conflict;
                payload.Message = "Existen registros relacionados que impiden esta operación.";
                break;
            case "23502": // not_null_violation
                payload.StatusCode = (int)HttpStatusCode.BadRequest;
                payload.Message = $"Falta un campo obligatorio: {pg.ColumnName ?? "(desconocido)"}.";
                break;
            case "23514": // check_violation
                payload.StatusCode = (int)HttpStatusCode.BadRequest;
                payload.Message = "Uno de los valores no cumple las reglas de negocio.";
                break;
            case "22001": // string_data_right_truncation
                payload.StatusCode = (int)HttpStatusCode.BadRequest;
                payload.Message = "Uno de los valores excede la longitud permitida.";
                break;
            case "40001": // serialization_failure
            case "40P01": // deadlock_detected
                payload.StatusCode = (int)HttpStatusCode.Conflict;
                payload.Message = "Conflicto de concurrencia. Intenta nuevamente.";
                break;
            case "57P03": // cannot_connect_now
            case "08006": // connection_failure
            case "08001": // sqlclient_unable_to_establish_sqlconnection
                payload.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                payload.Message = "La base de datos no está disponible. Intenta en unos segundos.";
                break;
            default:
                payload.StatusCode = (int)HttpStatusCode.InternalServerError;
                payload.Message = "Error de base de datos. Si persiste, contacta a soporte.";
                break;
        }
    }

    private static string BuildDetail(Exception exception)
    {
        var detail = $"{exception.GetType().Name}: {exception.Message}";
        var inner = exception.InnerException;
        var depth = 0;
        while (inner != null && depth < 3)
        {
            detail += $" --> {inner.GetType().Name}: {inner.Message}";
            inner = inner.InnerException;
            depth++;
        }
        return detail;
    }
}
