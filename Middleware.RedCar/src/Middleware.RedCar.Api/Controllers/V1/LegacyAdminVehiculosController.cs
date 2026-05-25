using System.Security.Claims;
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
[Route("api/v{version:apiVersion}/admin/Vehiculos")]
[Produces("application/json")]
public sealed class LegacyAdminVehiculosController : ControllerBase
{
    private readonly ICatalogoClient _catalogo;

    public LegacyAdminVehiculosController(ICatalogoClient catalogo) => _catalogo = catalogo;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _catalogo.ListInventarioAsync(ct: ct) ?? Array.Empty<VehiculoAdminDto>();
        return Ok(ApiResponse<object>.Ok(items.Select(LegacyAdminDtoMapper.ToVehiculo).ToList(), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles(CancellationToken ct)
    {
        var items = await _catalogo.ListInventarioAsync(ct: ct) ?? Array.Empty<VehiculoAdminDto>();
        var disponibles = items.Where(v => v.EstadoOperativo == "DISPONIBLE").Select(LegacyAdminDtoMapper.ToVehiculo).ToList();
        return Ok(ApiResponse<object>.Ok(disponibles, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var items = await _catalogo.ListInventarioAsync(ct: ct) ?? Array.Empty<VehiculoAdminDto>();
        var v = items.FirstOrDefault(x => x.IdVehiculo == id);
        return v is null
            ? NotFound(ApiResponse<object>.Fail(404, "Vehículo no encontrado", HttpContext.TraceIdentifier))
            : Ok(ApiResponse<object>.Ok(LegacyAdminDtoMapper.ToVehiculo(v), traceId: HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object body, CancellationToken ct)
    {
        try
        {
            var dto = await _catalogo.CreateVehiculoAsync(body, ct);
            return Ok(ApiResponse<object>.Ok(LegacyAdminDtoMapper.ToVehiculo(dto), "Vehículo creado exitosamente", HttpContext.TraceIdentifier));
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
            var dto = await _catalogo.UpdateVehiculoAsync(id, body, ct);
            return Ok(ApiResponse<object>.Ok(LegacyAdminDtoMapper.ToVehiculo(dto), "Vehículo actualizado exitosamente", HttpContext.TraceIdentifier));
        }
        catch (MicroserviceClientException ex)
        {
            return StatusCode((int)ex.StatusCode, ApiResponse<object>.Fail((int)ex.StatusCode, ex.Message, HttpContext.TraceIdentifier));
        }
    }

    [HttpPut("{id:int}/estado-operativo")]
    public async Task<IActionResult> CambiarEstadoOperativo(int id, [FromBody] CambiarEstadoVehiculoBody body, CancellationToken ct)
    {
        try
        {
            await _catalogo.CambiarEstadoOperativoVehiculoAsync(id, body.EstadoOperativo, ct);
            return Ok(ApiResponse<object>.Ok(new { id, estadoOperativo = body.EstadoOperativo }, traceId: HttpContext.TraceIdentifier));
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
            await _catalogo.DeleteVehiculoAsync(id, ct);
            return Ok(ApiResponse<object>.Ok(new { id }, "Vehículo eliminado exitosamente", HttpContext.TraceIdentifier));
        }
        catch (MicroserviceClientException ex)
        {
            return StatusCode((int)ex.StatusCode, ApiResponse<object>.Fail((int)ex.StatusCode, ex.Message, HttpContext.TraceIdentifier));
        }
    }
}

public sealed class CambiarEstadoVehiculoBody
{
    public string EstadoOperativo { get; set; } = string.Empty;
}
