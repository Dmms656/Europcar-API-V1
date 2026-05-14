namespace Europcar.Rental.DataManagement.Models;

public class VehiculoModel
{
    public int IdVehiculo { get; set; }
    public Guid VehiculoGuid { get; set; }
    public string CodigoInterno { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public int IdMarca { get; set; }
    public string Marca { get; set; } = string.Empty;
    public int IdCategoria { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public short AnioFabricacion { get; set; }
    public string Color { get; set; } = string.Empty;
    public string TipoCombustible { get; set; } = string.Empty;
    public string TipoTransmision { get; set; } = string.Empty;
    public short CapacidadPasajeros { get; set; }
    public short CapacidadMaletas { get; set; }
    public short NumeroPuertas { get; set; }
    public decimal PrecioBaseDia { get; set; }
    public int KilometrajeActual { get; set; }
    public bool AireAcondicionado { get; set; }
    public string EstadoOperativo { get; set; } = string.Empty;
    public string? ObservacionesGenerales { get; set; }
    public string? ImagenUrl { get; set; }
    public int IdLocalizacion { get; set; }
    public string NombreLocalizacion { get; set; } = string.Empty;
    public string EstadoVehiculo { get; set; } = "ACT";
    public long RowVersion { get; set; }
}
