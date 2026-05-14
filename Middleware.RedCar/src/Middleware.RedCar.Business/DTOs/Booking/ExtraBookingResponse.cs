namespace Middleware.RedCar.Business.DTOs.Booking;

/// <summary>
/// Item del Endpoint 7 (GET /api/v2/booking/extras). Forma exacta del contrato.
/// </summary>
public sealed class ExtraBookingResponse
{
    public int IdExtra { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal ValorFijo { get; set; }
    public string Estado { get; set; } = string.Empty;
}
