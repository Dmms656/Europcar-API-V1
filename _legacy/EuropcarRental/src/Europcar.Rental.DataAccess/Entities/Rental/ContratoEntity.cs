using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class ContratoEntity : BaseEntity
{
    public int IdContrato { get; set; }
    public Guid ContratoGuid { get; set; }
    public string NumeroContrato { get; set; } = string.Empty;
    public int IdReserva { get; set; }
    public int IdCliente { get; set; }
    public int IdVehiculo { get; set; }
    public DateTimeOffset FechaHoraSalida { get; set; }
    public DateTimeOffset FechaHoraPrevistaDevolucion { get; set; }
    public int KilometrajeSalida { get; set; }
    public decimal NivelCombustibleSalida { get; set; }
    public string EstadoContrato { get; set; } = "ABIERTO";
    public string? PdfUrl { get; set; }
    public string? ObservacionesContrato { get; set; }
    public string OrigenRegistro { get; set; } = string.Empty;

    // Navigation
    public ReservaEntity Reserva { get; set; } = null!;
    public ClienteEntity Cliente { get; set; } = null!;
    public VehiculoEntity Vehiculo { get; set; } = null!;
    public ICollection<CheckInOutEntity> Checks { get; set; } = new List<CheckInOutEntity>();
    public ICollection<PagoEntity> Pagos { get; set; } = new List<PagoEntity>();
}
