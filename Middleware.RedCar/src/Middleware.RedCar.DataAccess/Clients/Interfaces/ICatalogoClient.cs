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

    Task<IReadOnlyList<MarcaDto>?> ListMarcasAsync(CancellationToken ct = default);

    Task<IReadOnlyList<VehiculoAdminDto>?> ListInventarioAsync(int page = 1, int limit = 500, CancellationToken ct = default);

    Task<VehiculoAdminDto> CreateVehiculoAsync(object request, CancellationToken ct = default);

    Task<VehiculoAdminDto> UpdateVehiculoAsync(int id, object request, CancellationToken ct = default);

    Task CambiarEstadoOperativoVehiculoAsync(int id, string estadoOperativo, CancellationToken ct = default);

    Task DeleteVehiculoAsync(int id, CancellationToken ct = default);

    Task<ExtraDto> CreateExtraAsync(object request, CancellationToken ct = default);

    Task<ExtraDto> UpdateExtraAsync(int id, object request, CancellationToken ct = default);

    Task CambiarEstadoExtraAsync(int id, string estado, string? motivo, CancellationToken ct = default);

    Task DeleteExtraAsync(int id, CancellationToken ct = default);
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
    Guid ExtraGuid,
    string Codigo,
    string Nombre,
    string Descripcion,
    string TipoExtra,
    bool RequiereStock,
    decimal ValorFijo,
    string Estado);

public sealed record MarcaDto(int IdMarca, string Codigo, string Nombre, string Estado);

public sealed record VehiculoAdminDto(
    int IdVehiculo,
    Guid VehiculoGuid,
    string CodigoInterno,
    string Placa,
    int IdMarca,
    string Marca,
    int IdCategoria,
    string Categoria,
    string Modelo,
    short AnioFabricacion,
    string Color,
    string TipoCombustible,
    string TipoTransmision,
    short CapacidadPasajeros,
    short CapacidadMaletas,
    short NumeroPuertas,
    decimal PrecioBaseDia,
    int KilometrajeActual,
    bool AireAcondicionado,
    string EstadoOperativo,
    string? ObservacionesGenerales,
    string? ImagenReferencialUrl,
    int IdLocalizacion,
    string EstadoVehiculo,
    long RowVersion);

public sealed record PagedDto<T>(IReadOnlyList<T> Items, int PaginaActual, int TotalPaginas, int TotalElementos, int ElementosPorPagina);
