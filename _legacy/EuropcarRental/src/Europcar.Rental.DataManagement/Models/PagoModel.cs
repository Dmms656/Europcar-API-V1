namespace Europcar.Rental.DataManagement.Models;

public class PagoModel
{
    public int IdPago { get; set; }
    public Guid PagoGuid { get; set; }
    public string CodigoPago { get; set; } = string.Empty;
    public int? IdReserva { get; set; }
    public int? IdContrato { get; set; }
    public int IdCliente { get; set; }
    public string TipoPago { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = "PENDIENTE";
    public string? ReferenciaExterna { get; set; }
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "USD";
    public DateTimeOffset FechaPagoUtc { get; set; }
    public string? ObservacionesPago { get; set; }
    public string? NombreCliente { get; set; }
    public string? CodigoReserva { get; set; }
}
