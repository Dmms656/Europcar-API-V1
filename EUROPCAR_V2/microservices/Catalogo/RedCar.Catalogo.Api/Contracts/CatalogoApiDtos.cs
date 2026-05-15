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
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public decimal ValorFijo { get; init; }
    public string Estado { get; init; } = string.Empty;
}
