namespace Europcar.Rental.DataManagement.Models;

public class FacturaResumenModel
{
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTimeOffset FechaEmision { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ValorIva { get; set; }
    public decimal Total { get; set; }
    public string EstadoFactura { get; set; } = string.Empty;
    public string ServicioOrigen { get; set; } = string.Empty;
    public int? IdReserva { get; set; }
    public string? CodigoReserva { get; set; }
    public int? IdContrato { get; set; }
    public string? NumeroContrato { get; set; }
}
