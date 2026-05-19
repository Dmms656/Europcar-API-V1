using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedCar.Catalogo.Api.Contracts;
using RedCar.Catalogo.DataAccess.Context;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Catalogo.Api.Controllers;

[ApiController]
[Route("api/v1/categorias")]
public sealed class CategoriasController : ControllerBase
{
    private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(8);
    private readonly CatalogoDbContext _db;

    public CategoriasController(CatalogoDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedDto<CategoriaDto>>>> GetList(
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
            var rows = await _db.Categorias
                .AsNoTracking()
                .Where(c => !c.EsEliminado)
                .OrderBy(c => c.IdCategoria)
                .Skip((page - 1) * limit)
                .Take(limit + 1)
                .Select(c => new CategoriaDto
                {
                    IdCategoria = c.IdCategoria,
                    Codigo = c.CodigoCategoria,
                    Nombre = c.NombreCategoria,
                    Descripcion = c.DescripcionCategoria ?? string.Empty,
                    Estado = c.EstadoCategoria
                })
                .ToListAsync(timeoutCts.Token);

            var hasNext = rows.Count > limit;
            var items = hasNext ? rows.Take(limit).ToList() : rows;
            var paged = BuildPaged(items, page, limit, hasNext);

            return Ok(ApiResponse<PagedDto<CategoriaDto>>.Ok(paged, traceId: HttpContext.TraceIdentifier));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return StatusCode(504, ApiResponse<PagedDto<CategoriaDto>>.Fail(
                504,
                "Timeout consultando categorias.",
                HttpContext.TraceIdentifier));
        }
    }

    private static PagedDto<CategoriaDto> BuildPaged(IReadOnlyList<CategoriaDto> items, int page, int limit, bool hasNext)
    {
        var minimumTotal = ((page - 1) * limit) + items.Count + (hasNext ? 1 : 0);
        var totalPaginas = hasNext ? page + 1 : items.Count == 0 ? Math.Max(0, page - 1) : page;

        return new PagedDto<CategoriaDto>
        {
            Items = items,
            PaginaActual = page,
            TotalPaginas = totalPaginas,
            TotalElementos = minimumTotal,
            ElementosPorPagina = limit
        };
    }
}
