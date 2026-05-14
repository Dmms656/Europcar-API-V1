using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.Business.Exceptions;
using Middleware.RedCar.Business.Interfaces;
using Middleware.RedCar.Business.Mappers;
using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataManagement.Interfaces;
using Middleware.RedCar.DataManagement.Models.Localizaciones;

namespace Middleware.RedCar.Business.Orchestrators;

public sealed class MarketplaceOrchestrator : IMarketplaceOrchestrator
{
    private readonly ICatalogoDataService _catalogo;
    private readonly ILocalizacionesDataService _localizaciones;
    private readonly IReservasDataService _reservas;
    private readonly NegocioSettings _negocio;
    private readonly ILogger<MarketplaceOrchestrator> _logger;

    public MarketplaceOrchestrator(
        ICatalogoDataService catalogo,
        ILocalizacionesDataService localizaciones,
        IReservasDataService reservas,
        IOptions<NegocioSettings> negocio,
        ILogger<MarketplaceOrchestrator> logger)
    {
        _catalogo = catalogo;
        _localizaciones = localizaciones;
        _reservas = reservas;
        _negocio = negocio.Value;
        _logger = logger;
    }

    public async Task<(IReadOnlyList<VehiculoBookingResponse> Items, PaginacionResponse Paginacion)> BuscarVehiculosAsync(VehiculoFiltroRequest filtro, CancellationToken ct = default)
    {
        if (filtro.FechaDevolucion <= filtro.FechaRecogida)
            throw new ValidationException(new[] { new ValidationFailure("fechaDevolucion", "fechaDevolucion debe ser posterior a fechaRecogida.") });

        var page = filtro.Page <= 0 ? 1 : filtro.Page;
        var limit = filtro.Limit switch
        {
            <= 0 => 20,
            > 100 => 100,
            _ => filtro.Limit
        };

        var query = new VehiculoQuery(
            IdLocalizacion: filtro.IdLocalizacion,
            FechaRecogida: filtro.FechaRecogida,
            FechaDevolucion: filtro.FechaDevolucion,
            NombreCategoria: filtro.NombreCategoria,
            NombreMarca: filtro.NombreMarca,
            Transmision: filtro.Transmision,
            Sort: filtro.Sort,
            Page: page,
            Limit: limit);

        var (vehiculos, total) = await _catalogo.BuscarVehiculosAsync(query, ct);

        // Para cada vehiculo verificamos disponibilidad real y enriquecemos con datos de localizacion.
        LocalizacionDataModel? loc = null;
        try
        {
            loc = await _localizaciones.GetAsync(filtro.IdLocalizacion, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enriquecer localizacion {Id}", filtro.IdLocalizacion);
        }

        var responses = new List<VehiculoBookingResponse>(vehiculos.Count);
        foreach (var v in vehiculos)
        {
            bool disponible = true;
            try
            {
                disponible = await _reservas.VerificarDisponibilidadAsync(
                    v.IdVehiculo, filtro.IdLocalizacion, filtro.FechaRecogida, filtro.FechaDevolucion, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo verificacion disponibilidad para vehiculo {Id}; asumo disponible=true", v.IdVehiculo);
            }

            responses.Add(CatalogoBusinessMapper.ToBooking(
                v, filtro.FechaRecogida, filtro.FechaDevolucion, disponible,
                _negocio.IvaPorcentaje,
                loc?.Nombre ?? string.Empty,
                loc?.Direccion ?? string.Empty));
        }

        var paginacion = new PaginacionResponse
        {
            PaginaActual = page,
            ElementosPorPagina = limit,
            TotalElementos = total,
            TotalPaginas = limit == 0 ? 0 : (int)Math.Ceiling((double)total / limit)
        };

        return (responses, paginacion);
    }

    public async Task<VehiculoDetalleResponse> GetVehiculoDetalleAsync(int idVehiculo, CancellationToken ct = default)
    {
        var v = await _catalogo.GetVehiculoAsync(idVehiculo, ct)
            ?? throw new NotFoundException($"Vehiculo {idVehiculo} no encontrado.");

        LocalizacionDataModel? loc = null;
        try
        {
            loc = await _localizaciones.GetAsync(v.IdLocalizacion, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enriquecer localizacion {Id} en detalle", v.IdLocalizacion);
        }

        return CatalogoBusinessMapper.ToDetalle(
            v,
            loc?.Nombre ?? string.Empty,
            loc?.Codigo ?? string.Empty,
            loc?.Direccion ?? string.Empty);
    }

    public async Task<(IReadOnlyList<LocalizacionBookingResponse> Items, PaginacionResponse Paginacion)> ListLocalizacionesAsync(int? idCiudad, int page, int limit, CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        limit = limit switch { <= 0 => 20, > 100 => 100, _ => limit };

        var (items, total) = await _localizaciones.ListAsync(idCiudad, page, limit, ct);
        if (items.Count == 0 && total == 0)
            throw new NotFoundException("No se encontraron localizaciones.");

        var responses = items.Select(LocalizacionesBusinessMapper.ToBooking).ToList();
        var paginacion = new PaginacionResponse
        {
            PaginaActual = page,
            ElementosPorPagina = limit,
            TotalElementos = total,
            TotalPaginas = limit == 0 ? 0 : (int)Math.Ceiling((double)total / limit)
        };
        return (responses, paginacion);
    }

    public async Task<LocalizacionBookingResponse> GetLocalizacionAsync(int idLocalizacion, CancellationToken ct = default)
    {
        var loc = await _localizaciones.GetAsync(idLocalizacion, ct)
            ?? throw new NotFoundException($"Localizacion {idLocalizacion} no encontrada.");
        return LocalizacionesBusinessMapper.ToBooking(loc);
    }

    public async Task<(IReadOnlyList<CategoriaBookingResponse> Items, PaginacionResponse Paginacion)> ListCategoriasAsync(int page, int limit, CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        limit = limit switch { <= 0 => 20, > 100 => 100, _ => limit };

        var (items, total) = await _catalogo.ListCategoriasAsync(page, limit, ct);
        var responses = items.Select(CatalogoBusinessMapper.ToBooking).ToList();
        return (responses, BuildPaginacion(page, limit, total));
    }

    public async Task<(IReadOnlyList<ExtraBookingResponse> Items, PaginacionResponse Paginacion)> ListExtrasAsync(int page, int limit, CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        limit = limit switch { <= 0 => 50, > 100 => 100, _ => limit };

        var (items, total) = await _catalogo.ListExtrasAsync(page, limit, ct);
        var responses = items.Select(CatalogoBusinessMapper.ToBooking).ToList();
        return (responses, BuildPaginacion(page, limit, total));
    }

    public async Task<IReadOnlyList<CiudadPublicaDto>> ListCiudadesAsync(CancellationToken ct = default)
    {
        try
        {
            var (items, _) = await _localizaciones.ListAsync(null, 1, 500, ct);
            return items
                .GroupBy(x => x.IdCiudad)
                .Select(g => new CiudadPublicaDto(g.Key, 0, g.First().CiudadNombre, string.Empty))
                .OrderBy(c => c.NombreCiudad)
                .ToList();
        }
        catch (NotFoundException)
        {
            return Array.Empty<CiudadPublicaDto>();
        }
    }

    private static PaginacionResponse BuildPaginacion(int page, int limit, int total) => new()
    {
        PaginaActual = page,
        ElementosPorPagina = limit,
        TotalElementos = total,
        TotalPaginas = limit == 0 ? 0 : (int)Math.Ceiling((double)total / limit)
    };
}
