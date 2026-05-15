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

        var q = _db.Extras
            .AsNoTracking()
            .Where(e => !e.EsEliminado && e.EstadoExtra == "ACT")
            .OrderBy(e => e.IdExtra);

        var total = await q.CountAsync(ct);
        var rows = await q.Skip((page - 1) * limit).Take(limit).ToListAsync(ct);

        var items = rows.Select(Map).ToList();
        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit);

        var paged = new PagedDto<ExtraDto>
        {
            Items = items,
            PaginaActual = page,
            TotalPaginas = totalPaginas,
            TotalElementos = total,
            ElementosPorPagina = limit
        };

        return Ok(ApiResponse<PagedDto<ExtraDto>>.Ok(paged, traceId: HttpContext.TraceIdentifier));
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
            .Where(e => idList.Contains(e.IdExtra) && !e.EsEliminado && e.EstadoExtra == "ACT")
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
}
