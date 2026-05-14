namespace RedCar.Shared.Contracts.Common;

/// <summary>
/// Envoltorio uniforme para respuestas REST de todos los microservicios.
/// El orquestador y el frontend dependen de esta forma estable.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public string? TraceId { get; init; }
    public IReadOnlyList<ApiError>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null) => new()
    {
        Success = true,
        StatusCode = 200,
        Message = message,
        Data = data,
        TraceId = traceId
    };

    public static ApiResponse<T> Fail(int statusCode, string message, string? traceId = null, IReadOnlyList<ApiError>? errors = null) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Message = message,
        TraceId = traceId,
        Errors = errors
    };
}

public sealed record ApiError(string Code, string Message, string? Field = null);
