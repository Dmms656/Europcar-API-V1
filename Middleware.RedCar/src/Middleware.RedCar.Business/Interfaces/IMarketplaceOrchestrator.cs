using Middleware.RedCar.Business.DTOs.Booking;

namespace Middleware.RedCar.Business.Interfaces;

/// <summary>
/// Orquestador de las consultas publicas del Booking (vehiculos, localizaciones,
/// categorias, extras). Combina llamadas a MS.Catalogo, MS.Localizaciones y
/// MS.Reservas (disponibilidad) para devolver la forma del contrato.
/// </summary>
public interface IMarketplaceOrchestrator
{
    Task<(IReadOnlyList<VehiculoBookingResponse> Items, PaginacionResponse Paginacion)> BuscarVehiculosAsync(VehiculoFiltroRequest filtro, CancellationToken ct = default);
    Task<VehiculoDetalleResponse> GetVehiculoDetalleAsync(int idVehiculo, CancellationToken ct = default);
    Task<(IReadOnlyList<LocalizacionBookingResponse> Items, PaginacionResponse Paginacion)> ListLocalizacionesAsync(int? idCiudad, int page, int limit, CancellationToken ct = default);
    Task<LocalizacionBookingResponse> GetLocalizacionAsync(int idLocalizacion, CancellationToken ct = default);
    Task<(IReadOnlyList<CategoriaBookingResponse> Items, PaginacionResponse Paginacion)> ListCategoriasAsync(int page, int limit, CancellationToken ct = default);
    Task<(IReadOnlyList<ExtraBookingResponse> Items, PaginacionResponse Paginacion)> ListExtrasAsync(int page, int limit, CancellationToken ct = default);

    /// <summary>
    /// Ciudades distintas derivadas de las sucursales (equivalente a GET /api/v1/ciudades del monolito).
    /// idPais puede ser 0 si el MS no expone país aún.
    /// </summary>
    Task<IReadOnlyList<CiudadPublicaDto>> ListCiudadesAsync(CancellationToken ct = default);
}

public sealed record CiudadPublicaDto(int IdCiudad, int IdPais, string NombreCiudad, string NombrePais);
