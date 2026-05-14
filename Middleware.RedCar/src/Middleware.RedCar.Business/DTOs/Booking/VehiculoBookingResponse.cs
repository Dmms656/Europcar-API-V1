using System.Text.Json.Serialization;

namespace Middleware.RedCar.Business.DTOs.Booking;

/// <summary>
/// Item de la lista del Endpoint 1 (GET /api/v2/booking/vehiculos).
/// Forma EXACTA del contrato.
/// </summary>
public sealed class VehiculoBookingResponse
{
    public int IdVehiculo { get; set; }
    public string CodigoInterno { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Color { get; set; } = string.Empty;
    public string ImagenUrl { get; set; } = string.Empty;
    public string Transmision { get; set; } = string.Empty;
    public string Combustible { get; set; } = string.Empty;
    public int CapacidadPasajeros { get; set; }
    public int CapacidadMaletas { get; set; }
    public int NumeroPuertas { get; set; }
    public bool AireAcondicionado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public LocalizacionResumen Localizacion { get; set; } = new();
    public DisponibilidadResumen Disponibilidad { get; set; } = new();
    public PrecioResumen Precio { get; set; } = new();

    [JsonPropertyName("_links")]
    public Dictionary<string, LinkHref> _Links { get; set; } = new();
}

/// <summary>Respuesta del Endpoint 2 (GET /api/v2/booking/vehiculos/{id}): un vehiculo con detalle.</summary>
public sealed class VehiculoDetalleResponse
{
    public int IdVehiculo { get; set; }
    public string CodigoInterno { get; set; } = string.Empty;
    public MarcaResumen Marca { get; set; } = new();
    public CategoriaResumen Categoria { get; set; } = new();
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Color { get; set; } = string.Empty;
    public string ImagenUrl { get; set; } = string.Empty;
    public string Transmision { get; set; } = string.Empty;
    public string Combustible { get; set; } = string.Empty;
    public int CapacidadPasajeros { get; set; }
    public int CapacidadMaletas { get; set; }
    public int NumeroPuertas { get; set; }
    public bool AireAcondicionado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public LocalizacionResumen Localizacion { get; set; } = new();

    [JsonPropertyName("_links")]
    public Dictionary<string, LinkHref> _Links { get; set; } = new();
}

public sealed class MarcaResumen
{
    public int IdMarca { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public sealed class CategoriaResumen
{
    public int IdCategoria { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public sealed class LocalizacionResumen
{
    public int IdLocalizacion { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
}

public sealed class DisponibilidadResumen
{
    public DateTimeOffset FechaRecogida { get; set; }
    public DateTimeOffset FechaDevolucion { get; set; }
    public int CantidadDias { get; set; }
    public bool Disponible { get; set; }
}

public sealed class PrecioResumen
{
    public decimal PrecioBaseDia { get; set; }
    public decimal SubtotalVehiculo { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}

public sealed class LinkHref
{
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
}
