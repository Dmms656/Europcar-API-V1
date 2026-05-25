using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Api.Services;
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
    private readonly GuestClientService _guestClientService;

    public BookingReservasController(IBookingService bookingService, GuestClientService guestClientService)
    {
        _bookingService = bookingService;
        _guestClientService = guestClientService;
    }

    /// <summary>
    /// Crear o resolver cliente invitado para el flujo público de reserva.
    /// No devuelve PII si el cliente ya existe (solo idCliente y esNuevo).
    /// </summary>
    [HttpPost("guest-client")]
    [EnableRateLimiting("guest-client")]
    public async Task<IActionResult> GuestClient([FromBody] GuestClientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Cedula) || string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(ApiResponse<object>.Fail("Cédula y nombre son obligatorios"));

        var (idCliente, esNuevo) = await _guestClientService.ResolveGuestClientAsync(
            request.Cedula,
            request.Nombre,
            request.Apellido,
            request.Telefono,
            request.Correo,
            request.Direccion);

        var mensaje = esNuevo ? "Cliente creado exitosamente" : "Cliente registrado para continuar la reserva";
        return Ok(ApiResponse<object>.Ok(new { idCliente, esNuevo }, mensaje));
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

public sealed class GuestClientRequest
{
    public string Cedula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Apellido { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}
