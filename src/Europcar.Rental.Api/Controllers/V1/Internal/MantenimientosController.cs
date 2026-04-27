using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.DTOs.Request.Mantenimientos;
using Europcar.Rental.Business.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class MantenimientosController : ControllerBase
{
    private readonly IMantenimientoService _mantenimientoService;

    public MantenimientosController(IMantenimientoService mantenimientoService)
    {
        _mantenimientoService = mantenimientoService;
    }

    /// <summary>
    /// Obtener todos los mantenimientos.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mantenimientoService.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener un mantenimiento por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mantenimientoService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener mantenimientos por vehículo.
    /// </summary>
    [HttpGet("vehiculo/{idVehiculo:int}")]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> GetByVehiculo(int idVehiculo)
    {
        var result = await _mantenimientoService.GetByVehiculoIdAsync(idVehiculo);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Registrar un nuevo mantenimiento (envía el vehículo a taller).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> Create([FromBody] CrearMantenimientoRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _mantenimientoService.CreateAsync(request, usuario);
        return CreatedAtAction(nameof(GetById), new { id = result.IdMantenimiento },
            ApiResponse<object>.Ok(result, "Mantenimiento registrado exitosamente"));
    }

    /// <summary>
    /// Cerrar un mantenimiento (devuelve el vehículo a disponible).
    /// </summary>
    [HttpPut("{id:int}/cerrar")]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> Cerrar(int id)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _mantenimientoService.CerrarAsync(id, usuario);
        return Ok(ApiResponse<object>.Ok(result, "Mantenimiento cerrado exitosamente"));
    }
}
