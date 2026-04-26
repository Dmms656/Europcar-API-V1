using System.Net;
using System.Text.Json;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.Exceptions;

namespace Europcar.Rental.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case BusinessException businessEx:
                response.StatusCode = businessEx.StatusCode;
                errorResponse.StatusCode = businessEx.StatusCode;
                errorResponse.Message = businessEx.Message;
                _logger.LogWarning("Business exception: {Message}", businessEx.Message);
                break;

            case Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.Message = "Conflicto de concurrencia. El registro fue modificado por otro usuario.";
                _logger.LogWarning("Concurrency conflict detected");
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "Ha ocurrido un error interno en el servidor";
                errorResponse.Detail = $"{exception.GetType().Name}: {exception.Message}";
                if (exception.InnerException != null)
                    errorResponse.Detail += $" --> {exception.InnerException.GetType().Name}: {exception.InnerException.Message}";
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.InnerException?.Message ?? exception.Message);
                break;
        }

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(json);
    }
}
