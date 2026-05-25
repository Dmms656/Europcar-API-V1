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
            .Where(l => !l.EsEliminado);

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
            .Where(x => x.IdLocalizacion == id && !x.EsEliminado)
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

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LocalizacionDto>>> Create(
        [FromBody] CrearLocalizacionRequest req,
        CancellationToken ct)
    {
        var codigo = (req.CodigoLocalizacion ?? string.Empty).Trim().ToUpperInvariant();
        if (await _db.Localizaciones.AnyAsync(l => !l.EsEliminado && l.CodigoLocalizacion == codigo, ct))
            return Conflict(ApiResponse<LocalizacionDto>.Fail(409, "Código de localización duplicado", HttpContext.TraceIdentifier));

        if (!await _db.Ciudades.AnyAsync(c => c.IdCiudad == req.IdCiudad && !c.EsEliminado, ct))
            return BadRequest(ApiResponse<LocalizacionDto>.Fail(400, "Ciudad no encontrada", HttpContext.TraceIdentifier));

        var entity = new RedCar.Localizaciones.DataAccess.Entities.Localizacion
        {
            LocalizacionGuid = Guid.NewGuid(),
            CodigoLocalizacion = codigo,
            NombreLocalizacion = req.NombreLocalizacion.Trim(),
            IdCiudad = req.IdCiudad,
            DireccionLocalizacion = req.DireccionLocalizacion.Trim(),
            TelefonoContacto = req.TelefonoContacto.Trim(),
            CorreoContacto = req.CorreoContacto.Trim(),
            HorarioAtencion = req.HorarioAtencion.Trim(),
            ZonaHoraria = string.IsNullOrWhiteSpace(req.ZonaHoraria) ? "America/Guayaquil" : req.ZonaHoraria.Trim(),
            Latitud = req.Latitud,
            Longitud = req.Longitud,
            EstadoLocalizacion = "ACT",
            EsEliminado = false,
            FechaRegistroUtc = DateTimeOffset.UtcNow,
            CreadoPorUsuario = User?.Identity?.Name ?? "ADMIN_API",
            OrigenRegistro = "ADMIN_API"
        };
        _db.Localizaciones.Add(entity);
        await _db.SaveChangesAsync(ct);
        var created = await GetDtoAsync(entity.IdLocalizacion, ct);
        return Ok(ApiResponse<LocalizacionDto>.Ok(created!, "Localización creada exitosamente", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<LocalizacionDto>>> Update(
        int id,
        [FromBody] ActualizarLocalizacionRequest req,
        CancellationToken ct)
    {
        var entity = await _db.Localizaciones.FirstOrDefaultAsync(l => l.IdLocalizacion == id && !l.EsEliminado, ct);
        if (entity is null)
            return NotFound(ApiResponse<LocalizacionDto>.Fail(404, "Localización no encontrada", HttpContext.TraceIdentifier));

        entity.NombreLocalizacion = req.NombreLocalizacion.Trim();
        entity.IdCiudad = req.IdCiudad;
        entity.DireccionLocalizacion = req.DireccionLocalizacion.Trim();
        entity.TelefonoContacto = req.TelefonoContacto.Trim();
        entity.CorreoContacto = req.CorreoContacto.Trim();
        entity.HorarioAtencion = req.HorarioAtencion.Trim();
        entity.ZonaHoraria = string.IsNullOrWhiteSpace(req.ZonaHoraria) ? entity.ZonaHoraria : req.ZonaHoraria.Trim();
        entity.Latitud = req.Latitud;
        entity.Longitud = req.Longitud;
        entity.ModificadoPorUsuario = User?.Identity?.Name ?? "ADMIN_API";
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
        var dto = await GetDtoAsync(id, ct);
        return Ok(ApiResponse<LocalizacionDto>.Ok(dto!, "Localización actualizada exitosamente", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:int}/estado")]
    public async Task<ActionResult<ApiResponse<object>>> CambiarEstado(
        int id,
        [FromBody] CambiarEstadoRequest req,
        CancellationToken ct)
    {
        var entity = await _db.Localizaciones.FirstOrDefaultAsync(l => l.IdLocalizacion == id && !l.EsEliminado, ct);
        if (entity is null)
            return NotFound(ApiResponse<object>.Fail(404, "Localización no encontrada", HttpContext.TraceIdentifier));

        var estado = (req.Estado ?? "ACT").Trim().ToUpperInvariant();
        entity.EstadoLocalizacion = estado is "ACT" or "INA" ? estado : "ACT";
        entity.MotivoInhabilitacion = req.Motivo;
        entity.ModificadoPorUsuario = User?.Identity?.Name ?? "ADMIN_API";
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { id, estado = entity.EstadoLocalizacion }, traceId: HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct)
    {
        var entity = await _db.Localizaciones.FirstOrDefaultAsync(l => l.IdLocalizacion == id && !l.EsEliminado, ct);
        if (entity is null)
            return NotFound(ApiResponse<object>.Fail(404, "Localización no encontrada", HttpContext.TraceIdentifier));

        entity.EsEliminado = true;
        entity.EstadoLocalizacion = "INA";
        entity.ModificadoPorUsuario = User?.Identity?.Name ?? "ADMIN_API";
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { id }, "Localización eliminada exitosamente", HttpContext.TraceIdentifier));
    }

    private async Task<LocalizacionDto?> GetDtoAsync(int id, CancellationToken ct) =>
        await _db.Localizaciones.AsNoTracking()
            .Where(l => l.IdLocalizacion == id)
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
