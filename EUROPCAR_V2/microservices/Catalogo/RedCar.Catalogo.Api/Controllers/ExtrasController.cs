using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedCar.Catalogo.Api.Contracts;
using RedCar.Catalogo.DataAccess.Context;
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
                .Select(e => new ExtraDto
                {
                    IdExtra = e.IdExtra,
                    ExtraGuid = e.ExtraGuid,
                    Codigo = e.CodigoExtra,
                    Nombre = e.NombreExtra,
                    Descripcion = e.DescripcionExtra,
                    TipoExtra = e.TipoExtra,
                    RequiereStock = e.RequiereStock,
                    ValorFijo = e.ValorFijo,
                    Estado = e.EstadoExtra
                })
                .ToListAsync(timeoutCts.Token);

            var hasNext = rows.Count > limit;
            var items = hasNext ? rows.Take(limit).ToList() : rows;
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
            .Select(e => new ExtraDto
            {
                IdExtra = e.IdExtra,
                ExtraGuid = e.ExtraGuid,
                Codigo = e.CodigoExtra,
                Nombre = e.NombreExtra,
                Descripcion = e.DescripcionExtra,
                TipoExtra = e.TipoExtra,
                RequiereStock = e.RequiereStock,
                ValorFijo = e.ValorFijo,
                Estado = e.EstadoExtra
            })
            .ToListAsync(ct);

        var order = idList
            .Select(id => rows.FirstOrDefault(r => r.IdExtra == id))
            .Where(r => r is not null)
            .Select(r => r!)
            .ToList();

        return Ok(ApiResponse<IReadOnlyList<ExtraDto>>.Ok(order, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ExtraDto>>> GetById(int id, CancellationToken ct)
    {
        var dto = await MapExtraAsync(id, ct);
        return dto is null
            ? NotFound(ApiResponse<ExtraDto>.Fail(404, "Extra no encontrado", HttpContext.TraceIdentifier))
            : Ok(ApiResponse<ExtraDto>.Ok(dto, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExtraDto>>> Create([FromBody] CrearExtraRequest req, CancellationToken ct)
    {
        try
        {
            var usuario = User?.Identity?.Name ?? "ADMIN_API";
            var codigo = (req.CodigoExtra ?? string.Empty).Trim().ToUpperInvariant();
            if (await _db.Extras.AnyAsync(e => !e.EsEliminado && e.CodigoExtra == codigo, ct))
                return Conflict(ApiResponse<ExtraDto>.Fail(409, $"Código {codigo} ya existe", HttpContext.TraceIdentifier));

            var entity = new RedCar.Catalogo.DataAccess.Entities.Extra
            {
                ExtraGuid = Guid.NewGuid(),
                CodigoExtra = codigo,
                NombreExtra = req.NombreExtra.Trim(),
                DescripcionExtra = req.DescripcionExtra?.Trim() ?? string.Empty,
                TipoExtra = string.IsNullOrWhiteSpace(req.TipoExtra) ? "SERVICIO" : req.TipoExtra.Trim().ToUpperInvariant(),
                RequiereStock = req.RequiereStock,
                ValorFijo = req.ValorFijo,
                EstadoExtra = "ACT",
                EsEliminado = false,
                FechaRegistroUtc = DateTimeOffset.UtcNow,
                CreadoPorUsuario = usuario,
                OrigenRegistro = "ADMIN_API",
                RowVersion = 1
            };
            _db.Extras.Add(entity);
            await _db.SaveChangesAsync(ct);
            var dto = await MapExtraAsync(entity.IdExtra, ct);
            return Ok(ApiResponse<ExtraDto>.Ok(dto!, "Extra creado exitosamente", HttpContext.TraceIdentifier));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ExtraDto>.Fail(400, ex.Message, HttpContext.TraceIdentifier));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ExtraDto>>> Update(int id, [FromBody] ActualizarExtraRequest req, CancellationToken ct)
    {
        var entity = await _db.Extras.FirstOrDefaultAsync(e => e.IdExtra == id && !e.EsEliminado, ct);
        if (entity is null)
            return NotFound(ApiResponse<ExtraDto>.Fail(404, "Extra no encontrado", HttpContext.TraceIdentifier));

        entity.NombreExtra = req.NombreExtra.Trim();
        entity.DescripcionExtra = req.DescripcionExtra?.Trim() ?? string.Empty;
        entity.TipoExtra = string.IsNullOrWhiteSpace(req.TipoExtra) ? entity.TipoExtra : req.TipoExtra.Trim().ToUpperInvariant();
        entity.RequiereStock = req.RequiereStock;
        entity.ValorFijo = req.ValorFijo;
        entity.ModificadoPorUsuario = User?.Identity?.Name ?? "ADMIN_API";
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
        var dto = await MapExtraAsync(id, ct);
        return Ok(ApiResponse<ExtraDto>.Ok(dto!, "Extra actualizado exitosamente", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:int}/estado")]
    public async Task<ActionResult<ApiResponse<object>>> CambiarEstado(int id, [FromBody] CambiarEstadoRequest req, CancellationToken ct)
    {
        var entity = await _db.Extras.FirstOrDefaultAsync(e => e.IdExtra == id && !e.EsEliminado, ct);
        if (entity is null)
            return NotFound(ApiResponse<object>.Fail(404, "Extra no encontrado", HttpContext.TraceIdentifier));

        var estado = (req.Estado ?? "ACT").Trim().ToUpperInvariant();
        if (estado is not ("ACT" or "INA"))
            return BadRequest(ApiResponse<object>.Fail(400, "Estado debe ser ACT o INA", HttpContext.TraceIdentifier));

        entity.EstadoExtra = estado;
        entity.MotivoInhabilitacion = req.Motivo;
        entity.ModificadoPorUsuario = User?.Identity?.Name ?? "ADMIN_API";
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        if (estado == "INA") entity.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { id, estado }, traceId: HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct)
    {
        var entity = await _db.Extras.FirstOrDefaultAsync(e => e.IdExtra == id && !e.EsEliminado, ct);
        if (entity is null)
            return NotFound(ApiResponse<object>.Fail(404, "Extra no encontrado", HttpContext.TraceIdentifier));

        entity.EsEliminado = true;
        entity.EstadoExtra = "INA";
        entity.ModificadoPorUsuario = User?.Identity?.Name ?? "ADMIN_API";
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { id }, "Extra eliminado exitosamente", HttpContext.TraceIdentifier));
    }

    private async Task<ExtraDto?> MapExtraAsync(int id, CancellationToken ct) =>
        await _db.Extras.AsNoTracking()
            .Where(e => e.IdExtra == id)
            .Select(e => new ExtraDto
            {
                IdExtra = e.IdExtra,
                ExtraGuid = e.ExtraGuid,
                Codigo = e.CodigoExtra,
                Nombre = e.NombreExtra,
                Descripcion = e.DescripcionExtra,
                TipoExtra = e.TipoExtra,
                RequiereStock = e.RequiereStock,
                ValorFijo = e.ValorFijo,
                Estado = e.EstadoExtra
            })
            .FirstOrDefaultAsync(ct);

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
