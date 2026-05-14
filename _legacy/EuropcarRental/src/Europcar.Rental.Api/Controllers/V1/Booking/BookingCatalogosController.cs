using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Business.DTOs.Request.Booking;
using Europcar.Rental.Business.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Booking;

/// <summary>
/// Endpoints públicos de catálogos para integración con Booking / OTA.
/// Cumple el contrato de API externo (Endpoints 4, 5, 6 y 7).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public class BookingCatalogosController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingCatalogosController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Endpoint 4: Listar localizaciones (sucursales) con paginación.
    /// GET /api/v1/localizaciones?idCiudad=1&page=1&limit=20
    /// </summary>
    [HttpGet("localizaciones")]
    public async Task<IActionResult> GetLocalizaciones([FromQuery] BookingLocalizacionesRequest request)
    {
        var result = await _bookingService.GetLocalizacionesAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Endpoint 5: Detalle de una localización específica.
    /// GET /api/v1/localizaciones/{localizacionId}
    /// </summary>
    [HttpGet("localizaciones/{localizacionId:int}")]
    public async Task<IActionResult> GetLocalizacionDetalle(int localizacionId)
    {
        var result = await _bookingService.GetLocalizacionDetalleAsync(localizacionId);
        return Ok(result);
    }

    /// <summary>
    /// Endpoint complementario: Listar ciudades (incluye país) para filtros públicos.
    /// GET /api/v1/ciudades
    /// </summary>
    [HttpGet("ciudades")]
    public async Task<IActionResult> GetCiudades()
    {
        var result = await _bookingService.GetCiudadesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Endpoint 6: Listar categorías de vehículos.
    /// GET /api/v1/categorias
    /// </summary>
    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias()
    {
        var result = await _bookingService.GetCategoriasAsync();
        return Ok(result);
    }

    /// <summary>
    /// Endpoint 7: Listar extras disponibles con precio fijo.
    /// GET /api/v1/extras
    /// </summary>
    [HttpGet("extras")]
    public async Task<IActionResult> GetExtras()
    {
        var result = await _bookingService.GetExtrasAsync();
        return Ok(result);
    }
}
