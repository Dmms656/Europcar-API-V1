using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedCar.Localizaciones.Api.Contracts;
using RedCar.Localizaciones.DataAccess.Context;
using RedCar.Localizaciones.DataAccess.Entities;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Localizaciones.Api.Controllers;

[ApiController]
[Route("api/v1/localizaciones")]
public sealed class LocalizacionesController : ControllerBase
{
    private readonly LocalizacionesDbContext _db;

    public LocalizacionesController(LocalizacionesDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedDto<LocalizacionDto>>>> GetList(
        [FromQuery] int? idCiudad,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        limit = limit is < 1 or > 100 ? 50 : limit;
        page = page < 1 ? 1 : page;

        var q = _db.Localizaciones
            .AsNoTracking()
            .Include(l => l.Ciudad)
            .Where(l => !l.EsEliminado && l.EstadoLocalizacion == "ACT");

        if (idCiudad is > 0)
        {
            q = q.Where(l => l.IdCiudad == idCiudad.Value);
        }

        q = q.OrderBy(l => l.IdLocalizacion);

        var total = await q.CountAsync(ct);
        var rows = await q.Skip((page - 1) * limit).Take(limit).ToListAsync(ct);

        var items = rows.Select(Map).ToList();
        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit);

        var paged = new PagedDto<LocalizacionDto>
        {
            Items = items,
            PaginaActual = page,
            TotalPaginas = totalPaginas,
            TotalElementos = total,
            ElementosPorPagina = limit
        };

        return Ok(ApiResponse<PagedDto<LocalizacionDto>>.Ok(paged, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<LocalizacionDto>>> GetById(int id, CancellationToken ct)
    {
        var l = await _db.Localizaciones
            .AsNoTracking()
            .Include(x => x.Ciudad)
            .FirstOrDefaultAsync(x => x.IdLocalizacion == id && !x.EsEliminado && x.EstadoLocalizacion == "ACT", ct);

        if (l is null)
        {
            return NotFound(ApiResponse<LocalizacionDto>.Fail(404, "Localizacion no encontrada.", HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<LocalizacionDto>.Ok(Map(l), traceId: HttpContext.TraceIdentifier));
    }

    private static LocalizacionDto Map(Localizacion l) => new()
    {
        IdLocalizacion = l.IdLocalizacion,
        Codigo = l.CodigoLocalizacion,
        Nombre = l.NombreLocalizacion,
        Direccion = l.DireccionLocalizacion,
        Telefono = l.TelefonoContacto,
        Correo = l.CorreoContacto,
        HorarioAtencion = l.HorarioAtencion,
        ZonaHoraria = l.ZonaHoraria,
        Estado = l.EstadoLocalizacion,
        IdCiudad = l.IdCiudad,
        CiudadNombre = l.Ciudad?.NombreCiudad ?? string.Empty
    };
}
