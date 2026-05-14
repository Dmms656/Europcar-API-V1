using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.DTOs.Request.Localizaciones;
using Europcar.Rental.Business.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

/// <summary>
/// Gestión administrativa de localizaciones (sucursales).
/// Acceso restringido a roles ADMIN y AGENTE_POS.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/localizaciones")]
[Authorize(Roles = "ADMIN,AGENTE_POS")]
public class LocalizacionesController : ControllerBase
{
    private readonly ILocalizacionService _localizacionService;

    public LocalizacionesController(ILocalizacionService localizacionService)
    {
        _localizacionService = localizacionService;
    }

    /// <summary>
    /// Listar todas las localizaciones (incluye INACTIVAS por defecto para administración).
    /// </summary>
    /// <param name="soloActivas">Si true, solo retorna las localizaciones con estado ACT.</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool soloActivas = false)
    {
        var result = await _localizacionService.GetAllAsync(soloActivas);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener una localización por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _localizacionService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Listar las ciudades disponibles (para selector en formularios).
    /// </summary>
    [HttpGet("ciudades")]
    public async Task<IActionResult> GetCiudades()
    {
        var result = await _localizacionService.GetCiudadesAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Crear una nueva localización (sucursal). Solo ADMIN.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] CrearLocalizacionRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _localizacionService.CreateAsync(request, usuario);
        return CreatedAtAction(nameof(GetById), new { id = result.IdLocalizacion },
            ApiResponse<object>.Ok(result, "Localización creada exitosamente"));
    }

    /// <summary>
    /// Actualizar una localización existente. ADMIN y AGENTE_POS.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarLocalizacionRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _localizacionService.UpdateAsync(id, request, usuario);
        return Ok(ApiResponse<object>.Ok(result, "Localización actualizada exitosamente"));
    }

    /// <summary>
    /// Activar / inhabilitar una localización. Solo ADMIN.
    /// </summary>
    [HttpPut("{id:int}/estado")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoLocalizacionRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        await _localizacionService.CambiarEstadoAsync(id, request, usuario);
        return Ok(ApiResponse<object>.Ok(new { id, estado = request.Estado }, "Estado actualizado"));
    }

    /// <summary>
    /// Eliminar (soft-delete) una localización. Solo ADMIN.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        await _localizacionService.DeleteAsync(id, usuario);
        return Ok(ApiResponse<object>.Ok(new { id }, "Localización eliminada exitosamente"));
    }
}
