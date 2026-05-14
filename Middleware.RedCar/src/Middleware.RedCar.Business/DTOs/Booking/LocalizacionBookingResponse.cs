using System.Text.Json.Serialization;

namespace Middleware.RedCar.Business.DTOs.Booking;

/// <summary>
/// Item del Endpoint 4 (GET /api/v2/booking/localizaciones) y Endpoint 5 (detalle).
/// Forma EXACTA del contrato.
/// </summary>
public sealed class LocalizacionBookingResponse
{
    public int IdLocalizacion { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string HorarioAtencion { get; set; } = string.Empty;
    public string ZonaHoraria { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public CiudadResumen Ciudad { get; set; } = new();

    [JsonPropertyName("_links")]
    public Dictionary<string, LinkHref> _Links { get; set; } = new();
}

public sealed class CiudadResumen
{
    public int IdCiudad { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
