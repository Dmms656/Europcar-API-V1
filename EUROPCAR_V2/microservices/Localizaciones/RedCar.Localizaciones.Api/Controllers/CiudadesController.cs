using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedCar.Localizaciones.Api.Contracts;
using RedCar.Localizaciones.DataAccess.Context;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Localizaciones.Api.Controllers;

[ApiController]
[Route("api/v1/ciudades")]
public sealed class CiudadesController : ControllerBase
{
    private readonly LocalizacionesDbContext _db;

    public CiudadesController(LocalizacionesDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CiudadDto>>>> GetList(CancellationToken ct)
    {
        var rows = await _db.Ciudades
            .AsNoTracking()
            .Where(c => !c.EsEliminado)
            .OrderBy(c => c.NombreCiudad)
            .Select(c => new CiudadDto
            {
                IdCiudad = c.IdCiudad,
                CiudadGuid = c.CiudadGuid,
                IdPais = c.IdPais,
                NombreCiudad = c.NombreCiudad,
                EstadoCiudad = c.EstadoCiudad
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<IReadOnlyList<CiudadDto>>.Ok(rows, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("paises")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PaisDto>>>> GetPaises(CancellationToken ct)
    {
        var rows = await _db.Ciudades
            .AsNoTracking()
            .Where(c => !c.EsEliminado)
            .Select(c => c.IdPais)
            .Distinct()
            .OrderBy(id => id)
            .ToListAsync(ct);

        var paises = rows.Select(id => new PaisDto
        {
            Id = id,
            Codigo = id == 1 ? "EC" : $"P{id}",
            Nombre = id == 1 ? "Ecuador" : $"País {id}",
            Estado = "ACT"
        }).ToList();

        return Ok(ApiResponse<IReadOnlyList<PaisDto>>.Ok(paises, traceId: HttpContext.TraceIdentifier));
    }
}
