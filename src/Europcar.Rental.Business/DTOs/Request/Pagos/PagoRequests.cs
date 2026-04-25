namespace Europcar.Rental.Business.DTOs.Request.Pagos;

public class CrearPagoRequest
{
    public int? IdReserva { get; set; }
    public int? IdContrato { get; set; }
    public int IdCliente { get; set; }
    public string TipoPago { get; set; } = "COBRO";
    public string MetodoPago { get; set; } = "TARJETA";
    public decimal Monto { get; set; }
    public string? ReferenciaExterna { get; set; }
    public string? Observaciones { get; set; }
}
