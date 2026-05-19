using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedCar.Localizaciones.Api.Contracts;
using RedCar.Localizaciones.DataAccess.Context;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Localizaciones.Api.Controllers;

[ApiController]
[Route("api/v1/localizaciones")]
public sealed class LocalizacionesController : ControllerBase
{
    private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(8);
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

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(QueryTimeout);

        try
        {
            var q = _db.Localizaciones
            .AsNoTracking()
            .Where(l => !l.EsEliminado && l.EstadoLocalizacion == "ACT");

            if (idCiudad is > 0)
            {
                q = q.Where(l => l.IdCiudad == idCiudad.Value);
            }

            var rows = await q
                .OrderBy(l => l.IdLocalizacion)
                .Skip((page - 1) * limit)
                .Take(limit + 1)
                .Select(l => new LocalizacionDto
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
                    CiudadNombre = l.Ciudad != null ? l.Ciudad.NombreCiudad : string.Empty
                })
                .ToListAsync(timeoutCts.Token);

            var hasNext = rows.Count > limit;
            var items = hasNext ? rows.Take(limit).ToList() : rows;

            var paged = BuildPaged(items, page, limit, hasNext);
            return Ok(ApiResponse<PagedDto<LocalizacionDto>>.Ok(paged, traceId: HttpContext.TraceIdentifier));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return StatusCode(504, ApiResponse<PagedDto<LocalizacionDto>>.Fail(
                504,
                "Timeout consultando localizaciones.",
                HttpContext.TraceIdentifier));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<LocalizacionDto>>> GetById(int id, CancellationToken ct)
    {
        var dto = await _db.Localizaciones
            .AsNoTracking()
            .Where(x => x.IdLocalizacion == id && !x.EsEliminado && x.EstadoLocalizacion == "ACT")
            .Select(l => new LocalizacionDto
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
                CiudadNombre = l.Ciudad != null ? l.Ciudad.NombreCiudad : string.Empty
            })
            .FirstOrDefaultAsync(ct);

        if (dto is null)
        {
            return NotFound(ApiResponse<LocalizacionDto>.Fail(404, "Localizacion no encontrada.", HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<LocalizacionDto>.Ok(dto, traceId: HttpContext.TraceIdentifier));
    }

    private static PagedDto<LocalizacionDto> BuildPaged(IReadOnlyList<LocalizacionDto> items, int page, int limit, bool hasNext)
    {
        var minimumTotal = ((page - 1) * limit) + items.Count + (hasNext ? 1 : 0);
        var totalPaginas = hasNext ? page + 1 : items.Count == 0 ? Math.Max(0, page - 1) : page;

        return new PagedDto<LocalizacionDto>
        {
            Items = items,
            PaginaActual = page,
            TotalPaginas = totalPaginas,
            TotalElementos = minimumTotal,
            ElementosPorPagina = limit
        };
    }
}
