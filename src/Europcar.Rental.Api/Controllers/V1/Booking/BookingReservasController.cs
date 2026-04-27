using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Business.DTOs.Request.Booking;
using Europcar.Rental.Business.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Booking;

/// <summary>
/// Endpoints públicos de reservas para el contrato Booking / RedCar.
/// Implementa los endpoints 8, 9, 10 y 11 del contrato:
///   POST   /api/v1/reservas
///   GET    /api/v1/reservas/{codigoReserva}
///   PATCH  /api/v1/reservas/{codigoReserva}/cancelar
///   GET    /api/v1/reservas/{codigoReserva}/factura
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reservas")]
public class BookingReservasController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingReservasController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Endpoint 8: Crear una nueva reserva (público).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] BookingCrearReservaRequest request)
    {
        var result = await _bookingService.CrearReservaAsync(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Endpoint 9: Obtener detalle de una reserva por su código.
    /// </summary>
    [HttpGet("{codigoReserva}")]
    public async Task<IActionResult> GetByCodigo(string codigoReserva)
    {
        var result = await _bookingService.GetReservaByCodigoAsync(codigoReserva);
        return Ok(result);
    }

    /// <summary>
    /// Endpoint 10: Cancelar una reserva (PATCH).
    /// </summary>
    [HttpPatch("{codigoReserva}/cancelar")]
    public async Task<IActionResult> Cancelar(
        string codigoReserva,
        [FromBody] BookingCancelarReservaRequest request)
    {
        var usuario = User?.FindFirstValue(ClaimTypes.Name) ?? "BOOKING";
        var result = await _bookingService.CancelarReservaAsync(codigoReserva, request, usuario);
        return Ok(result);
    }

    /// <summary>
    /// Endpoint 11: Obtener la factura asociada a la reserva.
    /// </summary>
    [HttpGet("{codigoReserva}/factura")]
    public async Task<IActionResult> GetFactura(string codigoReserva)
    {
        var result = await _bookingService.GetFacturaPorReservaAsync(codigoReserva);
        return Ok(result);
    }
}
