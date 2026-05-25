using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedCar.Catalogo.Api.Contracts;
using RedCar.Catalogo.DataAccess.Context;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Catalogo.Api.Controllers;

[ApiController]
[Route("api/v1/vehiculos")]
public sealed class VehiculosController : ControllerBase
{
    private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(8);
    private readonly CatalogoDbContext _db;

    public VehiculosController(CatalogoDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedDto<VehiculoCatalogoDto>>>> GetList(
        [FromQuery] int idLocalizacion,
        [FromQuery] DateTimeOffset fechaRecogida,
        [FromQuery] DateTimeOffset fechaDevolucion,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? nombreCategoria = null,
        [FromQuery] string? nombreMarca = null,
        [FromQuery] string? transmision = null,
        [FromQuery] string? sort = null,
        CancellationToken ct = default)
    {
        _ = fechaRecogida;
        _ = fechaDevolucion;

        limit = limit is < 1 or > 100 ? 20 : limit;
        page = page < 1 ? 1 : page;

        var q = _db.Vehiculos
            .AsNoTracking()
            .Where(v => !v.EsEliminado && v.EstadoOperativo == "DISPONIBLE")
            .Where(v => v.LocalizacionActual == idLocalizacion);

        if (!string.IsNullOrWhiteSpace(nombreCategoria))
        {
            var nc = nombreCategoria.Trim();
            q = q.Where(v => v.Categoria != null && EF.Functions.ILike(v.Categoria.NombreCategoria, nc));
        }

        if (!string.IsNullOrWhiteSpace(nombreMarca))
        {
            var nm = nombreMarca.Trim();
            q = q.Where(v => v.Marca != null && EF.Functions.ILike(v.Marca.NombreMarca, nm));
        }

        if (!string.IsNullOrWhiteSpace(transmision))
        {
            var tr = transmision.Trim();
            q = q.Where(v => EF.Functions.ILike(v.TipoTransmision, $"%{tr}%"));
        }

        q = (sort ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "precio" or "precio_asc" => q.OrderBy(v => v.PrecioBaseDia).ThenBy(v => v.IdVehiculo),
            "preciodesc" or "precio_desc" => q.OrderByDescending(v => v.PrecioBaseDia).ThenBy(v => v.IdVehiculo),
            "marca" => q.OrderBy(v => v.Marca!.NombreMarca).ThenBy(v => v.ModeloVehiculo),
            "modelo" => q.OrderBy(v => v.ModeloVehiculo),
            _ => q.OrderBy(v => v.IdVehiculo)
        };

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(QueryTimeout);

        try
        {
            var rows = await q
                .Skip((page - 1) * limit)
                .Take(limit + 1)
                .Select(v => new VehiculoCatalogoDto
                {
                    IdVehiculo = v.IdVehiculo,
                    CodigoInterno = v.CodigoInternoVehiculo,
                    IdMarca = v.IdMarca,
                    Marca = v.Marca != null ? v.Marca.NombreMarca : string.Empty,
                    IdCategoria = v.IdCategoria,
                    CategoriaCodigo = v.Categoria != null ? v.Categoria.CodigoCategoria : string.Empty,
                    CategoriaNombre = v.Categoria != null ? v.Categoria.NombreCategoria : string.Empty,
                    Modelo = v.ModeloVehiculo,
                    Anio = v.AnioFabricacion,
                    Color = v.ColorVehiculo,
                    ImagenUrl = v.ImagenReferencialUrl ?? string.Empty,
                    Transmision = v.TipoTransmision,
                    Combustible = v.TipoCombustible,
                    CapacidadPasajeros = v.CapacidadPasajeros,
                    CapacidadMaletas = v.CapacidadMaletas,
                    NumeroPuertas = v.NumeroPuertas,
                    AireAcondicionado = v.AireAcondicionado,
                    Estado = v.EstadoOperativo,
                    IdLocalizacion = v.LocalizacionActual,
                    PrecioBaseDia = v.PrecioBaseDia
                })
                .ToListAsync(timeoutCts.Token);

            var hasNext = rows.Count > limit;
            var items = hasNext ? rows.Take(limit).ToList() : rows;
            var paged = BuildPaged(items, page, limit, hasNext);

            return Ok(ApiResponse<PagedDto<VehiculoCatalogoDto>>.Ok(paged, traceId: HttpContext.TraceIdentifier));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return StatusCode(504, ApiResponse<PagedDto<VehiculoCatalogoDto>>.Fail(
                504,
                "Timeout consultando vehiculos.",
                HttpContext.TraceIdentifier));
        }
    }

    [HttpGet("inventario")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<VehiculoAdminDto>>>> GetInventario(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 500,
        CancellationToken ct = default)
    {
        limit = limit is < 1 or > 1000 ? 500 : limit;
        page = page < 1 ? 1 : page;

        var rows = await _db.Vehiculos
            .AsNoTracking()
            .Where(v => !v.EsEliminado)
            .OrderBy(v => v.IdVehiculo)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(v => new VehiculoAdminDto
            {
                IdVehiculo = v.IdVehiculo,
                VehiculoGuid = v.VehiculoGuid,
                CodigoInterno = v.CodigoInternoVehiculo,
                Placa = v.PlacaVehiculo,
                IdMarca = v.IdMarca,
                Marca = v.Marca != null ? v.Marca.NombreMarca : string.Empty,
                IdCategoria = v.IdCategoria,
                Categoria = v.Categoria != null ? v.Categoria.NombreCategoria : string.Empty,
                Modelo = v.ModeloVehiculo,
                AnioFabricacion = v.AnioFabricacion,
                Color = v.ColorVehiculo,
                TipoCombustible = v.TipoCombustible,
                TipoTransmision = v.TipoTransmision,
                CapacidadPasajeros = v.CapacidadPasajeros,
                CapacidadMaletas = v.CapacidadMaletas,
                NumeroPuertas = v.NumeroPuertas,
                PrecioBaseDia = v.PrecioBaseDia,
                KilometrajeActual = v.KilometrajeActual,
                AireAcondicionado = v.AireAcondicionado,
                EstadoOperativo = v.EstadoOperativo,
                ObservacionesGenerales = v.ObservacionesGenerales,
                ImagenReferencialUrl = v.ImagenReferencialUrl,
                IdLocalizacion = v.LocalizacionActual,
                EstadoVehiculo = v.EstadoVehiculo,
                RowVersion = v.RowVersion
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<IReadOnlyList<VehiculoAdminDto>>.Ok(rows, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<VehiculoCatalogoDto>>> GetById(int id, CancellationToken ct)
    {
        var dto = await _db.Vehiculos
            .AsNoTracking()
            .Where(x => x.IdVehiculo == id && !x.EsEliminado)
            .Select(v => new VehiculoCatalogoDto
            {
                IdVehiculo = v.IdVehiculo,
                CodigoInterno = v.CodigoInternoVehiculo,
                IdMarca = v.IdMarca,
                Marca = v.Marca != null ? v.Marca.NombreMarca : string.Empty,
                IdCategoria = v.IdCategoria,
                CategoriaCodigo = v.Categoria != null ? v.Categoria.CodigoCategoria : string.Empty,
                CategoriaNombre = v.Categoria != null ? v.Categoria.NombreCategoria : string.Empty,
                Modelo = v.ModeloVehiculo,
                Anio = v.AnioFabricacion,
                Color = v.ColorVehiculo,
                ImagenUrl = v.ImagenReferencialUrl ?? string.Empty,
                Transmision = v.TipoTransmision,
                Combustible = v.TipoCombustible,
                CapacidadPasajeros = v.CapacidadPasajeros,
                CapacidadMaletas = v.CapacidadMaletas,
                NumeroPuertas = v.NumeroPuertas,
                AireAcondicionado = v.AireAcondicionado,
                Estado = v.EstadoOperativo,
                IdLocalizacion = v.LocalizacionActual,
                PrecioBaseDia = v.PrecioBaseDia
            })
            .FirstOrDefaultAsync(ct);

        if (dto is null)
        {
            return NotFound(ApiResponse<VehiculoCatalogoDto>.Fail(404, "Vehiculo no encontrado.", HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<VehiculoCatalogoDto>.Ok(dto, traceId: HttpContext.TraceIdentifier));
    }

    private static PagedDto<VehiculoCatalogoDto> BuildPaged(IReadOnlyList<VehiculoCatalogoDto> items, int page, int limit, bool hasNext)
    {
        var minimumTotal = ((page - 1) * limit) + items.Count + (hasNext ? 1 : 0);
        var totalPaginas = hasNext ? page + 1 : items.Count == 0 ? Math.Max(0, page - 1) : page;

        return new PagedDto<VehiculoCatalogoDto>
        {
            Items = items,
            PaginaActual = page,
            TotalPaginas = totalPaginas,
            TotalElementos = minimumTotal,
            ElementosPorPagina = limit
        };
    }
}
