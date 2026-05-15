namespace RedCar.Catalogo.DataAccess.Entities;

public sealed class Vehiculo
{
    public int IdVehiculo { get; set; }
    public string CodigoInternoVehiculo { get; set; } = string.Empty;
    public int IdMarca { get; set; }
    public MarcaVehiculo? Marca { get; set; }
    public int IdCategoria { get; set; }
    public CategoriaVehiculo? Categoria { get; set; }
    public string ModeloVehiculo { get; set; } = string.Empty;
    public short AnioFabricacion { get; set; }
    public string ColorVehiculo { get; set; } = string.Empty;
    public string TipoCombustible { get; set; } = string.Empty;
    public string TipoTransmision { get; set; } = string.Empty;
    public short CapacidadPasajeros { get; set; }
    public short CapacidadMaletas { get; set; }
    public short NumeroPuertas { get; set; }
    public int LocalizacionActual { get; set; }
    public decimal PrecioBaseDia { get; set; }
    public bool AireAcondicionado { get; set; }
    public string EstadoOperativo { get; set; } = "DISPONIBLE";
    public string? ImagenReferencialUrl { get; set; }
    public string EstadoVehiculo { get; set; } = "ACT";
    public bool EsEliminado { get; set; }
}
