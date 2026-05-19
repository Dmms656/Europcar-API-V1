using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedCar.Catalogo.Api.Contracts;
using RedCar.Catalogo.DataAccess.Context;
using RedCar.Catalogo.DataAccess.Entities;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Catalogo.Api.Controllers;

[ApiController]
[Route("api/v1/extras")]
public sealed class ExtrasController : ControllerBase
{
    private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(8);
    private readonly CatalogoDbContext _db;

    public ExtrasController(CatalogoDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedDto<ExtraDto>>>> GetList(
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
            var rows = await _db.Extras
                .AsNoTracking()
                .Where(e => !e.EsEliminado)
                .OrderBy(e => e.IdExtra)
                .Skip((page - 1) * limit)
                .Take(limit + 1)
                .ToListAsync(timeoutCts.Token);

            var hasNext = rows.Count > limit;
            var items = (hasNext ? rows.Take(limit) : rows).Select(Map).ToList();
            var paged = BuildPaged(items, page, limit, hasNext);

            return Ok(ApiResponse<PagedDto<ExtraDto>>.Ok(paged, traceId: HttpContext.TraceIdentifier));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return StatusCode(504, ApiResponse<PagedDto<ExtraDto>>.Fail(
                504,
                "Timeout consultando extras.",
                HttpContext.TraceIdentifier));
        }
    }

    [HttpGet("by-ids")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ExtraDto>>>> GetByIds([FromQuery] string? ids, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ids))
        {
            return Ok(ApiResponse<IReadOnlyList<ExtraDto>>.Ok(Array.Empty<ExtraDto>(), traceId: HttpContext.TraceIdentifier));
        }

        var idList = new List<int>();
        foreach (var part in ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out var id)) idList.Add(id);
        }

        if (idList.Count == 0)
        {
            return Ok(ApiResponse<IReadOnlyList<ExtraDto>>.Ok(Array.Empty<ExtraDto>(), traceId: HttpContext.TraceIdentifier));
        }

        var rows = await _db.Extras
            .AsNoTracking()
            .Where(e => idList.Contains(e.IdExtra) && !e.EsEliminado)
            .OrderBy(e => e.IdExtra)
            .ToListAsync(ct);

        var order = idList
            .Select(id => rows.FirstOrDefault(r => r.IdExtra == id))
            .Where(r => r is not null)
            .Select(r => Map(r!))
            .ToList();

        return Ok(ApiResponse<IReadOnlyList<ExtraDto>>.Ok(order, traceId: HttpContext.TraceIdentifier));
    }

    private static ExtraDto Map(Extra e) => new()
    {
        IdExtra = e.IdExtra,
        Codigo = e.CodigoExtra,
        Nombre = e.NombreExtra,
        Descripcion = e.DescripcionExtra,
        ValorFijo = e.ValorFijo,
        Estado = e.EstadoExtra
    };

    private static PagedDto<ExtraDto> BuildPaged(IReadOnlyList<ExtraDto> items, int page, int limit, bool hasNext)
    {
        var minimumTotal = ((page - 1) * limit) + items.Count + (hasNext ? 1 : 0);
        var totalPaginas = hasNext ? page + 1 : items.Count == 0 ? Math.Max(0, page - 1) : page;

        return new PagedDto<ExtraDto>
        {
            Items = items,
            PaginaActual = page,
            TotalPaginas = totalPaginas,
            TotalElementos = minimumTotal,
            ElementosPorPagina = limit
        };
    }
}
