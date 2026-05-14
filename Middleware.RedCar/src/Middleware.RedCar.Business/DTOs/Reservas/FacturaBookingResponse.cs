namespace Middleware.RedCar.Business.DTOs.Reservas;

/// <summary>
/// Response del Endpoint 11 (GET /api/v2/booking/reservas/{codigoReserva}/factura).
/// Inferido del _link "factura" del response del Endpoint 9.
/// </summary>
public sealed class FacturaBookingResponse
{
    public string NumeroFactura { get; set; } = string.Empty;
    public string CodigoReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaFacturaUtc { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "USD";
    public string? UrlPdf { get; set; }
}
