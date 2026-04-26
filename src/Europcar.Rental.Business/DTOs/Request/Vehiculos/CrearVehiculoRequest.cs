namespace Europcar.Rental.Business.DTOs.Request.Vehiculos;

/// <summary>
/// Request para crear un nuevo vehículo en la flota.
/// </summary>
public class CrearVehiculoRequest
{
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
    public int IdLocalizacion { get; set; }
    public decimal PrecioBaseDia { get; set; }
    public int KilometrajeActual { get; set; }
    public bool AireAcondicionado { get; set; } = true;
    public string? ObservacionesGenerales { get; set; }

    /// <summary>
    /// URL de la imagen referencial del vehículo (opcional al crear, editable después).
    /// </summary>
    public string? ImagenReferencialUrl { get; set; }
}
