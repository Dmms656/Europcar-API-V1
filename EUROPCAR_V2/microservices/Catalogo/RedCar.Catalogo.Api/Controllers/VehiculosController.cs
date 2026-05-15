using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedCar.Catalogo.Api.Contracts;
using RedCar.Catalogo.DataAccess.Context;
using RedCar.Catalogo.DataAccess.Entities;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Catalogo.Api.Controllers;

[ApiController]
[Route("api/v1/vehiculos")]
public sealed class VehiculosController : ControllerBase
{
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
            .Include(v => v.Marca)
            .Include(v => v.Categoria)
            .Where(v => !v.EsEliminado && v.EstadoVehiculo == "ACT" && v.EstadoOperativo == "DISPONIBLE")
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

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(ct);

        var dtoItems = items.Select(Map).ToList();
        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit);

        var paged = new PagedDto<VehiculoCatalogoDto>
        {
            Items = dtoItems,
            PaginaActual = page,
            TotalPaginas = totalPaginas,
            TotalElementos = total,
            ElementosPorPagina = limit
        };

        return Ok(ApiResponse<PagedDto<VehiculoCatalogoDto>>.Ok(paged, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<VehiculoCatalogoDto>>> GetById(int id, CancellationToken ct)
    {
        var v = await _db.Vehiculos
            .AsNoTracking()
            .Include(x => x.Marca)
            .Include(x => x.Categoria)
            .FirstOrDefaultAsync(x => x.IdVehiculo == id && !x.EsEliminado && x.EstadoVehiculo == "ACT", ct);

        if (v is null)
        {
            return NotFound(ApiResponse<VehiculoCatalogoDto>.Fail(404, "Vehiculo no encontrado.", HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<VehiculoCatalogoDto>.Ok(Map(v), traceId: HttpContext.TraceIdentifier));
    }

    private static VehiculoCatalogoDto Map(Vehiculo v) => new()
    {
        IdVehiculo = v.IdVehiculo,
        CodigoInterno = v.CodigoInternoVehiculo,
        IdMarca = v.IdMarca,
        Marca = v.Marca?.NombreMarca ?? string.Empty,
        IdCategoria = v.IdCategoria,
        CategoriaCodigo = v.Categoria?.CodigoCategoria ?? string.Empty,
        CategoriaNombre = v.Categoria?.NombreCategoria ?? string.Empty,
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
    };
}
