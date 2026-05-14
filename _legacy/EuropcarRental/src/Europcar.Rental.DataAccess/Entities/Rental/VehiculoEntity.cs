using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class VehiculoEntity : BaseEstadoEntity
{
    public int IdVehiculo { get; set; }
    public Guid VehiculoGuid { get; set; }
    public string CodigoInternoVehiculo { get; set; } = string.Empty;
    public string PlacaVehiculo { get; set; } = string.Empty;
    public int IdMarca { get; set; }
    public int IdCategoria { get; set; }
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
    public int KilometrajeActual { get; set; }
    public bool AireAcondicionado { get; set; } = true;
    public string EstadoOperativo { get; set; } = "DISPONIBLE";
    public string? ObservacionesGenerales { get; set; }
    public string? ImagenReferencialUrl { get; set; }
    public string EstadoVehiculo { get; set; } = "ACT";

    // Navigation
    public MarcaVehiculoEntity Marca { get; set; } = null!;
    public CategoriaVehiculoEntity Categoria { get; set; } = null!;
    public LocalizacionEntity Localizacion { get; set; } = null!;
    public ICollection<ReservaEntity> Reservas { get; set; } = new List<ReservaEntity>();
}
