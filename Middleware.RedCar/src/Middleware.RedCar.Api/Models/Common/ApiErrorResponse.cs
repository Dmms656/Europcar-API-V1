using System.Text.Json.Serialization;

namespace Middleware.RedCar.Api.Models.Common;

/// <summary>
/// Respuesta de error que sigue la misma envoltura del contrato pero sin "data".
/// </summary>
public sealed class ApiErrorResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Alias para el frontend legacy (parseApiError usa <c>message</c>).</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    [JsonPropertyName("errores")]
    public IReadOnlyList<ApiFieldError>? Errores { get; set; }

    /// <summary>Errores por campo en formato monolito (dict de arrays de strings).</summary>
    [JsonPropertyName("errors")]
    public IReadOnlyDictionary<string, string[]>? Errors { get; set; }
}

public sealed record ApiFieldError(
    [property: JsonPropertyName("campo")] string Campo,
    [property: JsonPropertyName("mensaje")] string Mensaje);
