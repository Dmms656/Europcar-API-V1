using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.Interfaces;

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
}
