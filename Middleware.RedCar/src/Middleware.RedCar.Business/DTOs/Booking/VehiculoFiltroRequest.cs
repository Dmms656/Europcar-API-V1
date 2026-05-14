using System.ComponentModel.DataAnnotations;

namespace Middleware.RedCar.Business.DTOs.Booking;

/// <summary>
/// Query params del Endpoint 1 (GET /api/v2/booking/vehiculos).
/// Mapeado a [FromQuery] en el controller, asi que los nombres tienen que ser
/// EXACTAMENTE los del contrato.
/// </summary>
public sealed class VehiculoFiltroRequest
{
    [Required]
    public int IdLocalizacion { get; set; }

    [Required]
    public DateTimeOffset FechaRecogida { get; set; }

    [Required]
    public DateTimeOffset FechaDevolucion { get; set; }

    public string? NombreCategoria { get; set; }
    public string? NombreMarca { get; set; }
    public string? Transmision { get; set; }

    /// <summary>"precio_asc" o "precio_desc". Default: precio_asc.</summary>
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;

    /// <summary>Default 20, maximo 100.</summary>
    public int Limit { get; set; } = 20;
}
