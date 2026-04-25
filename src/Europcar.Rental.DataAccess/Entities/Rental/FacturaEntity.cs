using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class FacturaEntity : BaseEntity
{
    public int IdFactura { get; set; }
    public Guid FacturaGuid { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public int? IdReserva { get; set; }
    public int? IdContrato { get; set; }
    public DateTimeOffset FechaEmision { get; set; } = DateTimeOffset.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal ValorIva { get; set; }
    public decimal Total { get; set; }
    public string? ObservacionesFactura { get; set; }
    public string? OrigenCanalFactura { get; set; }
    public string EstadoFactura { get; set; } = "EMITIDA";
    public string ServicioOrigen { get; set; } = string.Empty;
    public string? MotivoInhabilitacion { get; set; }

    // Navigation
    public ClienteEntity Cliente { get; set; } = null!;
    public ReservaEntity? Reserva { get; set; }
    public ContratoEntity? Contrato { get; set; }
}
