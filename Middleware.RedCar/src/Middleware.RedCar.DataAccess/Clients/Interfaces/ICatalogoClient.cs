namespace Middleware.RedCar.DataAccess.Clients.Interfaces;

/// <summary>
/// Cliente REST hacia MS.Catalogo (esquema "catalogo").
/// Expone los DTOs tal cual los devuelve el MS, sin transformacion.
/// </summary>
public interface ICatalogoClient
{
    Task<PagedDto<VehiculoCatalogoDto>?> BuscarVehiculosAsync(VehiculoQuery query, CancellationToken ct = default);
    Task<VehiculoCatalogoDto?> GetVehiculoAsync(int idVehiculo, CancellationToken ct = default);
    Task<PagedDto<CategoriaDto>?> ListCategoriasAsync(int page, int limit, CancellationToken ct = default);
    Task<PagedDto<ExtraDto>?> ListExtrasAsync(int page, int limit, CancellationToken ct = default);
    Task<IReadOnlyList<ExtraDto>?> GetExtrasByIdsAsync(IEnumerable<int> idExtras, CancellationToken ct = default);
}

public sealed record VehiculoQuery(
    int IdLocalizacion,
    DateTimeOffset FechaRecogida,
    DateTimeOffset FechaDevolucion,
    string? NombreCategoria,
    string? NombreMarca,
    string? Transmision,
    string? Sort,
    int Page,
    int Limit);

public sealed record VehiculoCatalogoDto(
    int IdVehiculo,
    string CodigoInterno,
    int IdMarca,
    string Marca,
    int IdCategoria,
    string CategoriaCodigo,
    string CategoriaNombre,
    string Modelo,
    int Anio,
    string Color,
    string ImagenUrl,
    string Transmision,
    string Combustible,
    int CapacidadPasajeros,
    int CapacidadMaletas,
    int NumeroPuertas,
    bool AireAcondicionado,
    string Estado,
    int IdLocalizacion,
    decimal PrecioBaseDia);

public sealed record CategoriaDto(
    int IdCategoria,
    string Codigo,
    string Nombre,
    string Descripcion,
    string Estado);

public sealed record ExtraDto(
    int IdExtra,
    string Codigo,
    string Nombre,
    string Descripcion,
    decimal ValorFijo,
    string Estado);

public sealed record PagedDto<T>(IReadOnlyList<T> Items, int PaginaActual, int TotalPaginas, int TotalElementos, int ElementosPorPagina);
