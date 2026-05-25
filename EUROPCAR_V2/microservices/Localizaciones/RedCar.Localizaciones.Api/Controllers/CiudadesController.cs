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

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CiudadDto>>> Create([FromBody] CrearCiudadRequest req, CancellationToken ct)
    {
        var entity = new RedCar.Localizaciones.DataAccess.Entities.Ciudad
        {
            CiudadGuid = Guid.NewGuid(),
            IdPais = req.IdPais > 0 ? req.IdPais : 1,
            NombreCiudad = req.NombreCiudad.Trim(),
            EstadoCiudad = "ACT",
            EsEliminado = false,
            FechaRegistroUtc = DateTimeOffset.UtcNow,
            CreadoPorUsuario = User?.Identity?.Name ?? "ADMIN_API",
            OrigenRegistro = "ADMIN_API"
        };
        _db.Ciudades.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<CiudadDto>.Ok(new CiudadDto
        {
            IdCiudad = entity.IdCiudad,
            CiudadGuid = entity.CiudadGuid,
            IdPais = entity.IdPais,
            NombreCiudad = entity.NombreCiudad,
            EstadoCiudad = entity.EstadoCiudad
        }, "Ciudad creada exitosamente", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CiudadDto>>> Update(int id, [FromBody] CrearCiudadRequest req, CancellationToken ct)
    {
        var entity = await _db.Ciudades.FirstOrDefaultAsync(c => c.IdCiudad == id && !c.EsEliminado, ct);
        if (entity is null)
            return NotFound(ApiResponse<CiudadDto>.Fail(404, "Ciudad no encontrada", HttpContext.TraceIdentifier));

        entity.NombreCiudad = req.NombreCiudad.Trim();
        entity.IdPais = req.IdPais > 0 ? req.IdPais : entity.IdPais;
        entity.ModificadoPorUsuario = User?.Identity?.Name ?? "ADMIN_API";
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<CiudadDto>.Ok(new CiudadDto
        {
            IdCiudad = entity.IdCiudad,
            CiudadGuid = entity.CiudadGuid,
            IdPais = entity.IdPais,
            NombreCiudad = entity.NombreCiudad,
            EstadoCiudad = entity.EstadoCiudad
        }, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:int}/estado")]
    public async Task<ActionResult<ApiResponse<object>>> CambiarEstado(int id, [FromBody] CambiarEstadoRequest req, CancellationToken ct)
    {
        var entity = await _db.Ciudades.FirstOrDefaultAsync(c => c.IdCiudad == id && !c.EsEliminado, ct);
        if (entity is null)
            return NotFound(ApiResponse<object>.Fail(404, "Ciudad no encontrada", HttpContext.TraceIdentifier));

        entity.EstadoCiudad = (req.Estado ?? "ACT").Trim().ToUpperInvariant() is "INA" ? "INA" : "ACT";
        entity.ModificadoPorUsuario = User?.Identity?.Name ?? "ADMIN_API";
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { id, estado = entity.EstadoCiudad }, traceId: HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct)
    {
        var entity = await _db.Ciudades.FirstOrDefaultAsync(c => c.IdCiudad == id && !c.EsEliminado, ct);
        if (entity is null)
            return NotFound(ApiResponse<object>.Fail(404, "Ciudad no encontrada", HttpContext.TraceIdentifier));

        entity.EsEliminado = true;
        entity.EstadoCiudad = "INA";
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { id }, traceId: HttpContext.TraceIdentifier));
    }
}
