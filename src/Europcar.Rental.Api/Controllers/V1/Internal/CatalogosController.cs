using Asp.Versioning;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.Business.DTOs.Request.Catalogos;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CatalogosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    /// <summary>
    /// Obtener todas las localizaciones (sucursales) activas.
    /// </summary>
    [HttpGet("localizaciones")]
    public async Task<IActionResult> GetLocalizaciones()
    {
        var result = await _catalogoService.GetLocalizacionesAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener una localización por ID.
    /// </summary>
    [HttpGet("localizaciones/{id:int}")]
    public async Task<IActionResult> GetLocalizacionById(int id)
    {
        var result = await _catalogoService.GetLocalizacionByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener todas las categorías de vehículos activas.
    /// </summary>
    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias()
    {
        var result = await _catalogoService.GetCategoriasAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener todas las marcas de vehículos activas.
    /// </summary>
    [HttpGet("marcas")]
    public async Task<IActionResult> GetMarcas()
    {
        var result = await _catalogoService.GetMarcasAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener todos los extras disponibles.
    /// </summary>
    [HttpGet("extras")]
    public async Task<IActionResult> GetExtras()
    {
        var result = await _catalogoService.GetExtrasAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener un extra por ID.
    /// </summary>
    [HttpGet("extras/{id:int}")]
    public async Task<IActionResult> GetExtraById(int id)
    {
        var result = await _catalogoService.GetExtraByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Crear un nuevo extra.
    /// </summary>
    [HttpPost("extras")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateExtra([FromBody] CrearExtraRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _catalogoService.CreateExtraAsync(request, usuario);
        return CreatedAtAction(nameof(GetExtraById), new { id = result.IdExtra },
            ApiResponse<object>.Ok(result, "Extra creado exitosamente"));
    }

    /// <summary>
    /// Actualizar un extra existente.
    /// </summary>
    [HttpPut("extras/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateExtra(int id, [FromBody] ActualizarExtraRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _catalogoService.UpdateExtraAsync(id, request, usuario);
        return Ok(ApiResponse<object>.Ok(result, "Extra actualizado exitosamente"));
    }

    /// <summary>
    /// Activar/Inhabilitar un extra.
    /// </summary>
    [HttpPut("extras/{id:int}/estado")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CambiarEstadoExtra(int id, [FromBody] CambiarEstadoExtraRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        await _catalogoService.CambiarEstadoExtraAsync(id, request, usuario);
        return Ok(ApiResponse<object>.Ok(new { id, estado = request.Estado }, "Estado actualizado"));
    }

    /// <summary>
    /// Eliminar (soft delete) un extra.
    /// </summary>
    [HttpDelete("extras/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteExtra(int id)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        await _catalogoService.DeleteExtraAsync(id, usuario);
        return Ok(ApiResponse<object>.Ok(new { id }, "Extra eliminado exitosamente"));
    }
}
