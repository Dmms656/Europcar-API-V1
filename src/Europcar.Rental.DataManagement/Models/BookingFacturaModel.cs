namespace Europcar.Rental.DataManagement.Models;

/// <summary>
/// Proyección de factura para el módulo Booking.
/// Contiene la información de cliente y reserva asociada lista para mapear al contrato.
/// </summary>
public class BookingFacturaModel
{
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoFactura { get; set; } = string.Empty;
    public DateTimeOffset FechaEmision { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ValorIva { get; set; }
    public decimal Total { get; set; }
    public string ClienteNombres { get; set; } = string.Empty;
    public string ClienteApellidos { get; set; } = string.Empty;
    public string ClienteIdentificacion { get; set; } = string.Empty;
}
