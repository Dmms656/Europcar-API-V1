namespace Europcar.Rental.Api.Models.Common;

/// <summary>
/// Respuesta API estandarizada para todos los endpoints.
/// Tanto OK como Error usan exactamente el mismo shape para que
/// el frontend pueda procesarlos de forma uniforme.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    /// <summary>StatusCode HTTP (espejado en payload para clientes que sólo leen body).</summary>
    public int StatusCode { get; set; }

    /// <summary>Errores por campo en validaciones. Null si no hay.</summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; set; }

    /// <summary>Trace/correlation id útil para soporte y logs.</summary>
    public string? TraceId { get; set; }

    /// <summary>Detalle adicional del error (sólo en entornos no-Producción).</summary>
    public string? Detail { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa") => new()
    {
        Success = true,
        Message = message,
        Data = data,
        StatusCode = 200
    };

    public static ApiResponse<T> Fail(string message, int statusCode = 400) => new()
    {
        Success = false,
        Message = message,
        StatusCode = statusCode
    };
}

/// <summary>
/// Respuesta de error genérica (alias semántico de ApiResponse&lt;object&gt; para errores).
/// Mantiene compatibilidad hacia atrás.
/// </summary>
public class ErrorResponse : ApiResponse<object>
{
    public ErrorResponse()
    {
        Success = false;
    }
}
