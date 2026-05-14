using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class PagoEntity : BaseEntity
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
    public DateTimeOffset FechaPagoUtc { get; set; } = DateTimeOffset.UtcNow;
    public string? ObservacionesPago { get; set; }
    public string OrigenRegistro { get; set; } = string.Empty;

    // Navigation
    public ReservaEntity? Reserva { get; set; }
    public ContratoEntity? Contrato { get; set; }
    public ClienteEntity Cliente { get; set; } = null!;
}
