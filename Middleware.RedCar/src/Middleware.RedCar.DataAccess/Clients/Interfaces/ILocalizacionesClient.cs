namespace Middleware.RedCar.DataAccess.Clients.Interfaces;

/// <summary>
/// Cliente REST hacia MS.Localizaciones (esquema "localizaciones").
/// </summary>
public interface ILocalizacionesClient
{
    Task<PagedDto<LocalizacionDto>?> ListLocalizacionesAsync(int? idCiudad, int page, int limit, CancellationToken ct = default);
    Task<LocalizacionDto?> GetLocalizacionAsync(int idLocalizacion, CancellationToken ct = default);

    Task<IReadOnlyList<CiudadDto>?> ListCiudadesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<PaisDto>?> ListPaisesAsync(CancellationToken ct = default);

    Task<LocalizacionDto> CreateLocalizacionAsync(object request, CancellationToken ct = default);

    Task<LocalizacionDto> UpdateLocalizacionAsync(int id, object request, CancellationToken ct = default);

    Task CambiarEstadoLocalizacionAsync(int id, string estado, string? motivo, CancellationToken ct = default);

    Task DeleteLocalizacionAsync(int id, CancellationToken ct = default);

    Task<CiudadDto> CreateCiudadAsync(object request, CancellationToken ct = default);

    Task<CiudadDto> UpdateCiudadAsync(int id, object request, CancellationToken ct = default);

    Task CambiarEstadoCiudadAsync(int id, string estado, CancellationToken ct = default);

    Task DeleteCiudadAsync(int id, CancellationToken ct = default);
}

public sealed record CiudadDto(int IdCiudad, Guid CiudadGuid, int IdPais, string NombreCiudad, string EstadoCiudad);

public sealed record PaisDto(int Id, string Codigo, string Nombre, string Estado);

public sealed record LocalizacionDto(
    int IdLocalizacion,
    string Codigo,
    string Nombre,
    string Direccion,
    string Telefono,
    string Correo,
    string HorarioAtencion,
    string ZonaHoraria,
    string Estado,
    int IdCiudad,
    string CiudadNombre);
