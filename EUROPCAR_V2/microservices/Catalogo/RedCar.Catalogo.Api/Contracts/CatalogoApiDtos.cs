namespace RedCar.Catalogo.Api.Contracts;

/// <summary>Forma JSON compatible con Middleware.RedCar (camelCase).</summary>
public sealed class PagedDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int PaginaActual { get; init; }
    public int TotalPaginas { get; init; }
    public int TotalElementos { get; init; }
    public int ElementosPorPagina { get; init; }
}

public sealed class VehiculoCatalogoDto
{
    public int IdVehiculo { get; init; }
    public string CodigoInterno { get; init; } = string.Empty;
    public int IdMarca { get; init; }
    public string Marca { get; init; } = string.Empty;
    public int IdCategoria { get; init; }
    public string CategoriaCodigo { get; init; } = string.Empty;
    public string CategoriaNombre { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public int Anio { get; init; }
    public string Color { get; init; } = string.Empty;
    public string ImagenUrl { get; init; } = string.Empty;
    public string Transmision { get; init; } = string.Empty;
    public string Combustible { get; init; } = string.Empty;
    public int CapacidadPasajeros { get; init; }
    public int CapacidadMaletas { get; init; }
    public int NumeroPuertas { get; init; }
    public bool AireAcondicionado { get; init; }
    public string Estado { get; init; } = string.Empty;
    public int IdLocalizacion { get; init; }
    public decimal PrecioBaseDia { get; init; }
}

public sealed class CategoriaDto
{
    public int IdCategoria { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
}

public sealed class ExtraDto
{
    public int IdExtra { get; init; }
    public Guid ExtraGuid { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string TipoExtra { get; init; } = "SERVICIO";
    public bool RequiereStock { get; init; }
    public decimal ValorFijo { get; init; }
    public string Estado { get; init; } = string.Empty;
}

public sealed class MarcaDto
{
    public int IdMarca { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
}

/// <summary>Inventario completo para panel administrativo.</summary>
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
    public string? ImagenReferencialUrl { get; set; }
}

public sealed class ActualizarVehiculoRequest : CrearVehiculoRequest
{
    public long RowVersion { get; set; }
}

public sealed class CambiarEstadoVehiculoRequest
{
    public string EstadoOperativo { get; set; } = string.Empty;
}

public sealed class CrearExtraRequest
{
    public string CodigoExtra { get; set; } = string.Empty;
    public string NombreExtra { get; set; } = string.Empty;
    public string? DescripcionExtra { get; set; }
    public string TipoExtra { get; set; } = "SERVICIO";
    public bool RequiereStock { get; set; }
    public decimal ValorFijo { get; set; }
}

public sealed class ActualizarExtraRequest
{
    public string NombreExtra { get; set; } = string.Empty;
    public string? DescripcionExtra { get; set; }
    public string TipoExtra { get; set; } = "SERVICIO";
    public bool RequiereStock { get; set; }
    public decimal ValorFijo { get; set; }
}

public sealed class CambiarEstadoRequest
{
    public string Estado { get; set; } = "ACT";
    public string? Motivo { get; set; }
}

public sealed class VehiculoAdminDto
{
    public int IdVehiculo { get; init; }
    public Guid VehiculoGuid { get; init; }
    public string CodigoInterno { get; init; } = string.Empty;
    public string Placa { get; init; } = string.Empty;
    public int IdMarca { get; init; }
    public string Marca { get; init; } = string.Empty;
    public int IdCategoria { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public short AnioFabricacion { get; init; }
    public string Color { get; init; } = string.Empty;
    public string TipoCombustible { get; init; } = string.Empty;
    public string TipoTransmision { get; init; } = string.Empty;
    public short CapacidadPasajeros { get; init; }
    public short CapacidadMaletas { get; init; }
    public short NumeroPuertas { get; init; }
    public decimal PrecioBaseDia { get; init; }
    public int KilometrajeActual { get; init; }
    public bool AireAcondicionado { get; init; }
    public string EstadoOperativo { get; init; } = string.Empty;
    public string? ObservacionesGenerales { get; init; }
    public string? ImagenReferencialUrl { get; init; }
    public int IdLocalizacion { get; init; }
    public string EstadoVehiculo { get; init; } = string.Empty;
    public long RowVersion { get; init; }
}
