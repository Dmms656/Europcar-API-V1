using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class ReservaConductorEntity : BaseEntity
{
    public int IdReservaConductor { get; set; }
    public Guid ReservaConductorGuid { get; set; }
    public int IdReserva { get; set; }
    public int IdConductor { get; set; }
    public string TipoConductor { get; set; } = "ADICIONAL";
    public bool EsPrincipal { get; set; } = false;
    public decimal CargoConductorJoven { get; set; } = 0;
    public string EstadoReservaConductor { get; set; } = "ACT";
    public string OrigenRegistro { get; set; } = string.Empty;

    // Navigation
    public ReservaEntity Reserva { get; set; } = null!;
    public ConductorEntity Conductor { get; set; } = null!;
}
