using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.DTOs.Request.Vehiculos;
using Europcar.Rental.Business.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class VehiculosController : ControllerBase
{
    private readonly IVehiculoService _vehiculoService;

    public VehiculosController(IVehiculoService vehiculoService)
    {
        _vehiculoService = vehiculoService;
    }

    /// <summary>
    /// Buscar vehículos disponibles con filtros opcionales.
    /// </summary>
    [HttpGet("disponibles")]
    public async Task<IActionResult> GetDisponibles([FromQuery] BuscarVehiculosRequest request)
    {
        var result = await _vehiculoService.GetDisponiblesAsync(request);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener detalle de un vehículo por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _vehiculoService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }
}
