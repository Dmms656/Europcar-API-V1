using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedCar.Catalogo.Api.Contracts;
using RedCar.Catalogo.DataAccess.Context;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Catalogo.Api.Controllers;

[ApiController]
[Route("api/v1/marcas")]
public sealed class MarcasController : ControllerBase
{
    private readonly CatalogoDbContext _db;

    public MarcasController(CatalogoDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MarcaDto>>>> GetList(CancellationToken ct)
    {
        var rows = await _db.Marcas
            .AsNoTracking()
            .Where(m => !m.EsEliminado)
            .OrderBy(m => m.NombreMarca)
            .Select(m => new MarcaDto
            {
                IdMarca = m.IdMarca,
                Codigo = m.CodigoMarca,
                Nombre = m.NombreMarca,
                Estado = m.EstadoMarca
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<IReadOnlyList<MarcaDto>>.Ok(rows, traceId: HttpContext.TraceIdentifier));
    }
}
