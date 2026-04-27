using System.Security.Claims;
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
    /// Obtener todos los vehículos activos de la flota.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _vehiculoService.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Buscar vehículos disponibles con filtros opcionales.
    /// </summary>
    [HttpGet("disponibles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDisponibles([FromQuery] BuscarVehiculosRequest request)
    {
        var result = await _vehiculoService.GetDisponiblesAsync(request);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener detalle de un vehículo por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _vehiculoService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Crear un nuevo vehículo en la flota.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> Create([FromBody] CrearVehiculoRequest request)
    {
        var result = await _vehiculoService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.IdVehiculo },
            ApiResponse<object>.Ok(result, "Vehículo creado exitosamente"));
    }

    /// <summary>
    /// Actualizar un vehículo existente (incluye imagen referencial).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarVehiculoRequest request)
    {
        var result = await _vehiculoService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.Ok(result, "Vehículo actualizado exitosamente"));
    }

    /// <summary>
    /// Eliminar un vehículo (soft-delete lógico).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        await _vehiculoService.DeleteAsync(id, usuario);
        return Ok(ApiResponse<object>.Ok(new { Id = id }, "Vehículo eliminado exitosamente"));
    }
}
