namespace Europcar.Rental.Business.DTOs.Request.Mantenimientos;

public class CrearMantenimientoRequest
{
    public int IdVehiculo { get; set; }
    public string TipoMantenimiento { get; set; } = "PREVENTIVO";
    public int KilometrajeMantenimiento { get; set; }
    public decimal CostoMantenimiento { get; set; }
    public string? ProveedorTaller { get; set; }
    public string? Observaciones { get; set; }
}
