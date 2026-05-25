using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Compatibility;
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
    [HttpPut("{id:int}")]
    [HttpPut("{id:int}/estado-operativo")]
    [HttpDelete("{id:int}")]
    public IActionResult NotImplementedWrite()
        => StatusCode(501, ApiResponse<object>.Fail(501, "CRUD de vehículos no implementado en middleware.", HttpContext.TraceIdentifier));
}
