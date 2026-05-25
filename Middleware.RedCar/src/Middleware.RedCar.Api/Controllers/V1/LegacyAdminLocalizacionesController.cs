using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Compatibility;
using Middleware.RedCar.DataAccess.Clients;
using Middleware.RedCar.DataAccess.Clients.Interfaces;
using RedCar.Shared.Contracts.Common;

namespace Middleware.RedCar.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/admin/Localizaciones")]
[Produces("application/json")]
public sealed class LegacyAdminLocalizacionesController : ControllerBase
{
    private readonly ILocalizacionesClient _localizaciones;

    public LegacyAdminLocalizacionesController(ILocalizacionesClient localizaciones) => _localizaciones = localizaciones;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool soloActivas = false, CancellationToken ct = default)
    {
        var dtos = new List<LocalizacionDto>();
        var page = 1;
        while (page <= 20)
        {
            var paged = await _localizaciones.ListLocalizacionesAsync(null, page, 100, ct);
            if (paged?.Items is null || paged.Items.Count == 0) break;
            dtos.AddRange(paged.Items);
            if (paged.Items.Count < 100) break;
            page++;
        }

        if (soloActivas)
            dtos = dtos.Where(l => l.Estado == "ACT").ToList();

        var all = dtos.Select(LegacyAdminDtoMapper.ToLocalizacion).ToList();
        return Ok(ApiResponse<object>.Ok(all, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("ciudades")]
    public async Task<IActionResult> GetCiudades(CancellationToken ct)
    {
        var items = await _localizaciones.ListCiudadesAsync(ct) ?? Array.Empty<CiudadDto>();
        return Ok(ApiResponse<object>.Ok(items.Select(LegacyAdminDtoMapper.ToCiudad).ToList(), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var loc = await _localizaciones.GetLocalizacionAsync(id, ct);
        return loc is null
            ? NotFound(ApiResponse<object>.Fail(404, "Localización no encontrada", HttpContext.TraceIdentifier))
            : Ok(ApiResponse<object>.Ok(LegacyAdminDtoMapper.ToLocalizacion(loc), traceId: HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object body, CancellationToken ct)
    {
        try
        {
            var dto = await _localizaciones.CreateLocalizacionAsync(body, ct);
            return Ok(ApiResponse<object>.Ok(LegacyAdminDtoMapper.ToLocalizacion(dto), "Localización creada exitosamente", HttpContext.TraceIdentifier));
        }
        catch (MicroserviceClientException ex)
        {
            return StatusCode((int)ex.StatusCode, ApiResponse<object>.Fail((int)ex.StatusCode, ex.Message, HttpContext.TraceIdentifier));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] object body, CancellationToken ct)
    {
        try
        {
            var dto = await _localizaciones.UpdateLocalizacionAsync(id, body, ct);
            return Ok(ApiResponse<object>.Ok(LegacyAdminDtoMapper.ToLocalizacion(dto), "Localización actualizada exitosamente", HttpContext.TraceIdentifier));
        }
        catch (MicroserviceClientException ex)
        {
            return StatusCode((int)ex.StatusCode, ApiResponse<object>.Fail((int)ex.StatusCode, ex.Message, HttpContext.TraceIdentifier));
        }
    }

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] LegacyCambiarEstadoBody body, CancellationToken ct)
    {
        try
        {
            await _localizaciones.CambiarEstadoLocalizacionAsync(id, body.Estado, body.Motivo, ct);
            return Ok(ApiResponse<object>.Ok(new { id, estado = body.Estado }, traceId: HttpContext.TraceIdentifier));
        }
        catch (MicroserviceClientException ex)
        {
            return StatusCode((int)ex.StatusCode, ApiResponse<object>.Fail((int)ex.StatusCode, ex.Message, HttpContext.TraceIdentifier));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            await _localizaciones.DeleteLocalizacionAsync(id, ct);
            return Ok(ApiResponse<object>.Ok(new { id }, "Localización eliminada exitosamente", HttpContext.TraceIdentifier));
        }
        catch (MicroserviceClientException ex)
        {
            return StatusCode((int)ex.StatusCode, ApiResponse<object>.Fail((int)ex.StatusCode, ex.Message, HttpContext.TraceIdentifier));
        }
    }
}
