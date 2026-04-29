namespace Europcar.Rental.Business.DTOs.Response.Pagos;

public class PagoResponse
{
    public int IdPago { get; set; }
    public Guid PagoGuid { get; set; }
    public string CodigoPago { get; set; } = string.Empty;
    public int? IdReserva { get; set; }
    public int? IdContrato { get; set; }
    public int IdCliente { get; set; }
    public string TipoPago { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "USD";
    public DateTimeOffset FechaPagoUtc { get; set; }
    public string? ReferenciaExterna { get; set; }
    public string? NombreCliente { get; set; }
    public string? CodigoReserva { get; set; }
    public string? ObservacionesPago { get; set; }
}
