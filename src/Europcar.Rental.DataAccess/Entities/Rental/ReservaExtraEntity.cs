using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class ReservaExtraEntity : BaseEntity
{
    public int IdReservaExtra { get; set; }
    public Guid ReservaExtraGuid { get; set; }
    public int IdReserva { get; set; }
    public int IdExtra { get; set; }
    public int Cantidad { get; set; }
    public decimal ValorUnitarioExtra { get; set; }
    public decimal SubtotalExtra { get; set; }
    public string EstadoReservaExtra { get; set; } = "ACT";
    public string OrigenRegistro { get; set; } = string.Empty;

    // Navigation
    public ReservaEntity Reserva { get; set; } = null!;
    public ExtraEntity Extra { get; set; } = null!;
}
