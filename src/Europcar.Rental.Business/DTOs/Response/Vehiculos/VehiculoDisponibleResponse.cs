namespace Europcar.Rental.Business.DTOs.Response.Vehiculos;

public class VehiculoDisponibleResponse
{
    public int IdVehiculo { get; set; }
    public Guid VehiculoGuid { get; set; }
    public string CodigoInterno { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public short AnioFabricacion { get; set; }
    public string Color { get; set; } = string.Empty;
    public string TipoCombustible { get; set; } = string.Empty;
    public string TipoTransmision { get; set; } = string.Empty;
    public short CapacidadPasajeros { get; set; }
    public short CapacidadMaletas { get; set; }
    public decimal PrecioBaseDia { get; set; }
    public bool AireAcondicionado { get; set; }
    public string? ImagenUrl { get; set; }
    public int IdLocalizacion { get; set; }
    public string Localizacion { get; set; } = string.Empty;
}
