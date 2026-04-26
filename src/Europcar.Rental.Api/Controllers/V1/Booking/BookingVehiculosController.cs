using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Business.DTOs.Request.Booking;
using Europcar.Rental.Business.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Booking;

/// <summary>
/// Endpoints públicos de vehículos para integración con Booking / OTA.
/// Cumple el contrato de API externo (Endpoints 1, 2 y 3).
/// No requiere [Authorize] ya que es una API pública para terceros.
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
    /// GET /api/v1/vehiculos?idLocalizacion=1&fechaRecogida=...&fechaDevolucion=...
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BuscarVehiculos([FromQuery] BookingBuscarVehiculosRequest request)
    {
        try
        {
            var result = await _bookingService.BuscarVehiculosAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            var detail = $"{ex.GetType().Name}: {ex.Message}";
            if (ex.InnerException != null)
                detail += $" | Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
            return StatusCode(500, new { error = detail, stack = ex.StackTrace?.Substring(0, System.Math.Min(500, ex.StackTrace?.Length ?? 0)) });
        }
    }

    /// <summary>
    /// Endpoint 2: Detalle completo de un vehículo específico.
    /// GET /api/v1/vehiculos/{vehiculoId}
    /// </summary>
    [HttpGet("{vehiculoId:int}")]
    public async Task<IActionResult> GetDetalle(int vehiculoId)
    {
        var result = await _bookingService.GetVehiculoDetalleAsync(vehiculoId);
        return Ok(result);
    }

    /// <summary>
    /// Endpoint 3: Verificar disponibilidad en tiempo real de un vehículo.
    /// GET /api/v1/vehiculos/{vehiculoId}/disponibilidad?fechaRecogida=...&fechaDevolucion=...&idLocalizacion=...
    /// </summary>
    [HttpGet("{vehiculoId:int}/disponibilidad")]
    public async Task<IActionResult> VerificarDisponibilidad(
        int vehiculoId, [FromQuery] BookingDisponibilidadRequest request)
    {
        var result = await _bookingService.VerificarDisponibilidadAsync(vehiculoId, request);
        return Ok(result);
    }
}
