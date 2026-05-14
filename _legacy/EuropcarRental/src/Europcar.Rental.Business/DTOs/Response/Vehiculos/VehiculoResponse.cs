namespace Europcar.Rental.Business.DTOs.Response.Vehiculos;

/// <summary>
/// Respuesta completa de un vehículo para operaciones CRUD de gestión interna.
/// Incluye campos de administración como IDs de FK, estado, observaciones y RowVersion.
/// </summary>
public class VehiculoResponse
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

    /// <summary>
    /// URL de la imagen referencial del vehículo. 
    /// Puede ser actualizada desde el frontend para subir una nueva imagen.
    /// </summary>
    public string? ImagenReferencialUrl { get; set; }

    public int IdLocalizacion { get; set; }
    public string Localizacion { get; set; } = string.Empty;
    public string EstadoVehiculo { get; set; } = string.Empty;
    public long RowVersion { get; set; }
}
