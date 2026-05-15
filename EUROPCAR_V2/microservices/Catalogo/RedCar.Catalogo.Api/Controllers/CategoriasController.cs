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

        var q = _db.Categorias
            .AsNoTracking()
            .Where(c => !c.EsEliminado && c.EstadoCategoria == "ACT")
            .OrderBy(c => c.IdCategoria);

        var total = await q.CountAsync(ct);
        var rows = await q.Skip((page - 1) * limit).Take(limit).ToListAsync(ct);

        var items = rows.Select(c => new CategoriaDto
        {
            IdCategoria = c.IdCategoria,
            Codigo = c.CodigoCategoria,
            Nombre = c.NombreCategoria,
            Descripcion = c.DescripcionCategoria ?? string.Empty,
            Estado = c.EstadoCategoria
        }).ToList();

        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit);

        var paged = new PagedDto<CategoriaDto>
        {
            Items = items,
            PaginaActual = page,
            TotalPaginas = totalPaginas,
            TotalElementos = total,
            ElementosPorPagina = limit
        };

        return Ok(ApiResponse<PagedDto<CategoriaDto>>.Ok(paged, traceId: HttpContext.TraceIdentifier));
    }
}
