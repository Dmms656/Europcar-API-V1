using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Business.DTOs.Request.Booking;
using Europcar.Rental.Business.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Booking;

/// <summary>
/// Endpoints públicos de vehículos para integración con Booking / OTA.
/// Cumple el contrato de API externo (Endpoints 1, 2 y 3).
/// vehiculoId es un identificador alfanumérico (CodigoInternoVehiculo, p.ej. "veh-001").
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/vehiculos")]
public class BookingVehiculosController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingVehiculosController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Endpoint 1: Búsqueda paginada de vehículos disponibles.
    /// GET /api/v1/vehiculos?idLocalizacion=1&amp;fechaRecogida=...&amp;fechaDevolucion=...
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BuscarVehiculos([FromQuery] BookingBuscarVehiculosRequest request)
    {
        var result = await _bookingService.BuscarVehiculosAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Endpoint 2: Detalle completo de un vehículo específico.
    /// GET /api/v1/vehiculos/{vehiculoId}
    /// </summary>
    [HttpGet("{vehiculoId}")]
    public async Task<IActionResult> GetDetalle(string vehiculoId)
    {
        var result = await _bookingService.GetVehiculoDetalleAsync(vehiculoId);
        return Ok(result);
    }

    /// <summary>
    /// Endpoint 3: Verificar disponibilidad en tiempo real de un vehículo.
    /// GET /api/v1/vehiculos/{vehiculoId}/disponibilidad?fechaRecogida=...&amp;fechaDevolucion=...&amp;idLocalizacion=...
    /// </summary>
    [HttpGet("{vehiculoId}/disponibilidad")]
    public async Task<IActionResult> VerificarDisponibilidad(
        string vehiculoId, [FromQuery] BookingDisponibilidadRequest request)
    {
        var result = await _bookingService.VerificarDisponibilidadAsync(vehiculoId, request);
        return Ok(result);
    }
}
