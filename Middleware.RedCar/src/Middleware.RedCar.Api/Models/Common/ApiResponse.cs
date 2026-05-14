using System.Text.Json.Serialization;

namespace Middleware.RedCar.Api.Models.Common;

/// <summary>
/// Envoltorio de respuesta EXACTO al contrato del Endpoint Base:
/// <code>
/// { "status": 200, "mensaje": "Operación exitosa", "data": { ... } }
/// </code>
/// </summary>
public sealed class ApiResponse<T>
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string mensaje = "Operación exitosa") => new()
    {
        Status = 200,
        Mensaje = mensaje,
        Data = data
    };

    public static ApiResponse<T> Created(T data, string mensaje = "Reserva creada exitosamente") => new()
    {
        Status = 201,
        Mensaje = mensaje,
        Data = data
    };
}
