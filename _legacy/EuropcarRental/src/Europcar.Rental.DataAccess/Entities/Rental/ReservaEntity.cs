using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class ReservaEntity : BaseEntity
{
    public int IdReserva { get; set; }
    public Guid ReservaGuid { get; set; }
    public string CodigoReserva { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public int IdVehiculo { get; set; }
    public int IdLocalizacionRecogida { get; set; }
    public int IdLocalizacionDevolucion { get; set; }
    public string CanalReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaHoraRecogida { get; set; }
    public DateTimeOffset FechaHoraDevolucion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ValorImpuestos { get; set; }
    public decimal ValorExtras { get; set; }
    public decimal ValorDepositoGarantia { get; set; }
    public decimal CargoOneWay { get; set; }
    public decimal Total { get; set; }
    public string CodigoConfirmacion { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = "PENDIENTE";
    public bool RequiereHold { get; set; } = true;
    public DateTimeOffset? FechaCancelacionUtc { get; set; }
    public string? MotivoCancelacion { get; set; }
    public string OrigenRegistro { get; set; } = string.Empty;

    // Navigation
    public ClienteEntity Cliente { get; set; } = null!;
    public VehiculoEntity Vehiculo { get; set; } = null!;
    public LocalizacionEntity LocalizacionRecogida { get; set; } = null!;
    public LocalizacionEntity LocalizacionDevolucion { get; set; } = null!;
    public ICollection<ReservaExtraEntity> Extras { get; set; } = new List<ReservaExtraEntity>();
    public ICollection<ReservaConductorEntity> Conductores { get; set; } = new List<ReservaConductorEntity>();
}
