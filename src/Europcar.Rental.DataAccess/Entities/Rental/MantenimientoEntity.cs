namespace Europcar.Rental.DataAccess.Entities.Rental;

public class MantenimientoEntity
{
    public int IdMantenimiento { get; set; }
    public Guid MantenimientoGuid { get; set; }
    public string CodigoMantenimiento { get; set; } = string.Empty;
    public int IdVehiculo { get; set; }
    public string TipoMantenimiento { get; set; } = string.Empty;
    public DateTimeOffset FechaInicioUtc { get; set; }
    public DateTimeOffset? FechaFinUtc { get; set; }
    public int KilometrajeMantenimiento { get; set; }
    public decimal CostoMantenimiento { get; set; }
    public string? ProveedorTaller { get; set; }
    public string EstadoMantenimiento { get; set; } = "ABIERTO";
    public string? Observaciones { get; set; }
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public DateTimeOffset FechaRegistroUtc { get; set; } = DateTimeOffset.UtcNow;
    public long RowVersion { get; set; } = 1;

    // Navigation
    public VehiculoEntity Vehiculo { get; set; } = null!;
}
