namespace Europcar.Rental.Business.DTOs.Response.Mantenimientos;

public class MantenimientoResponse
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
    public string EstadoMantenimiento { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? PlacaVehiculo { get; set; }
}
