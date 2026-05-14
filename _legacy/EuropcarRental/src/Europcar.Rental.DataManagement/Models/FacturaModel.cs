namespace Europcar.Rental.DataManagement.Models;

public class FacturaModel
{
    public int IdFactura { get; set; }
    public Guid FacturaGuid { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public int? IdReserva { get; set; }
    public int? IdContrato { get; set; }
    public DateTimeOffset FechaEmision { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ValorIva { get; set; }
    public decimal Total { get; set; }
    public string EstadoFactura { get; set; } = "EMITIDA";
    public string ServicioOrigen { get; set; } = string.Empty;
    public string? OrigenCanalFactura { get; set; }
    public string? ObservacionesFactura { get; set; }
}
