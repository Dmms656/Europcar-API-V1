using System.Text.Json;
using Middleware.RedCar.Api.Models.Common;
using Middleware.RedCar.Business.Exceptions;

namespace Middleware.RedCar.Api.Middleware;

/// <summary>
/// Traduce excepciones de negocio del middleware a respuestas HTTP usando el
/// wrapper del contrato (ApiErrorResponse). Garantiza que ningun stacktrace
/// se filtre al canal de Booking.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException vex)
        {
            _logger.LogWarning("Validacion fallida: {Msg}", vex.Message);
            await WriteAsync(context, 422, vex.Message, vex.Failures.Select(f => new ApiFieldError(f.Field, f.Message)).ToList());
        }
        catch (NotFoundException nfx)
        {
            _logger.LogInformation("404: {Msg}", nfx.Message);
            await WriteAsync(context, 404, nfx.Message);
        }
        catch (ConflictException cex)
        {
            _logger.LogWarning("409: {Msg}", cex.Message);
            await WriteAsync(context, 409, cex.Message);
        }
        catch (UnauthorizedBusinessException uex)
        {
            _logger.LogWarning("401: {Msg}", uex.Message);
            await WriteAsync(context, 401, uex.Message);
        }
        catch (BusinessException bex)
        {
            _logger.LogWarning("Business error {Code}: {Msg}", bex.HttpStatusCode, bex.Message);
            await WriteAsync(context, bex.HttpStatusCode, bex.Message);
        }
        catch (TimeoutException tex)
        {
            _logger.LogError(tex, "Timeout llamando a un microservicio.");
            await WriteAsync(context, 504, "Timeout llamando al servicio downstream.");
        }
        catch (HttpRequestException hex)
        {
            _logger.LogError(hex, "Error de comunicacion con microservicio.");
            await WriteAsync(context, 502, "Error al comunicarse con un servicio interno.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepcion no controlada.");
            await WriteAsync(context, 500, "Error interno del servidor.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string mensaje, IReadOnlyList<ApiFieldError>? errores = null)
    {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";

        var errorsDict = errores is { Count: > 0 }
            ? errores.ToDictionary(e => e.Campo, e => new[] { e.Mensaje })
            : null;

        var body = new ApiErrorResponse
        {
            Status = status,
            StatusCode = status,
            Success = false,
            Mensaje = mensaje,
            Message = mensaje,
            TraceId = context.TraceIdentifier,
            Errores = errores,
            Errors = errorsDict
        };
        var json = JsonSerializer.Serialize(body, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}
