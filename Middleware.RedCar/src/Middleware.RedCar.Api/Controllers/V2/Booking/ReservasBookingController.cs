using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Models.Common;
using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.Business.DTOs.Reservas;
using Middleware.RedCar.Business.Interfaces;

namespace Middleware.RedCar.Api.Controllers.V2.Booking;

/// <summary>
/// Endpoints 3, 8, 9, 10 y 11 del contrato (todos los que cuelgan de /reservas).
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/booking/reservas")]
[Produces("application/json")]
public sealed class ReservasBookingController : ControllerBase
{
    private readonly IReservaOrchestrator _reservas;
    private readonly IFacturaOrchestrator _factura;

    public ReservasBookingController(IReservaOrchestrator reservas, IFacturaOrchestrator factura)
    {
        _reservas = reservas;
        _factura = factura;
    }

    // ==========================================================================
    // Endpoint 3 - GET /api/v2/booking/reservas/{idVehiculo}/disponibilidad
    // ==========================================================================
    [HttpGet("{idVehiculo:int}/disponibilidad")]
    [ProducesResponseType(typeof(ApiResponse<DisponibilidadResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 422)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<ActionResult<ApiResponse<DisponibilidadResponse>>> VerificarDisponibilidad(
        [FromRoute] int idVehiculo,
        [FromQuery] DateTimeOffset fechaRecogida,
        [FromQuery] DateTimeOffset fechaDevolucion,
        [FromQuery] int idLocalizacion,
        CancellationToken ct)
    {
        var disponibilidad = await _reservas.VerificarDisponibilidadAsync(
            idVehiculo, idLocalizacion, fechaRecogida, fechaDevolucion, ct);

        return Ok(ApiResponse<DisponibilidadResponse>.Ok(disponibilidad));
    }

    // ==========================================================================
    // Endpoint 8 - POST /api/v2/booking/reservas
    // ==========================================================================
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CrearReservaBookingResponse>), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    [ProducesResponseType(typeof(ApiErrorResponse), 422)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<ActionResult<ApiResponse<CrearReservaBookingResponse>>> Crear(
        [FromBody] CrearReservaBookingRequest request,
        CancellationToken ct)
    {
        var resultado = await _reservas.CrearReservaAsync(request, ct);
        var body = ApiResponse<CrearReservaBookingResponse>.Created(resultado);
        return new ObjectResult(body) { StatusCode = 201 };
    }

    // ==========================================================================
    // Endpoint 9 - GET /api/v2/booking/reservas/{codigoReserva}
    // ==========================================================================
    [HttpGet("{codigoReserva}")]
    [ProducesResponseType(typeof(ApiResponse<ReservaBookingResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<ActionResult<ApiResponse<ReservaBookingResponse>>> GetReserva(
        [FromRoute] string codigoReserva,
        CancellationToken ct)
    {
        var reserva = await _reservas.GetReservaAsync(codigoReserva, ct);
        return Ok(ApiResponse<ReservaBookingResponse>.Ok(reserva));
    }

    // ==========================================================================
    // Endpoint 10 (implicito) - PATCH /api/v2/booking/reservas/{codigoReserva}/cancelar
    // ==========================================================================
    [HttpPatch("{codigoReserva}/cancelar")]
    [ProducesResponseType(typeof(ApiResponse<CancelarReservaResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 422)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<ActionResult<ApiResponse<CancelarReservaResponse>>> Cancelar(
        [FromRoute] string codigoReserva,
        [FromBody] CancelarReservaRequest request,
        CancellationToken ct)
    {
        var usuario = User?.Identity?.Name ?? "BOOKING";
        var resultado = await _reservas.CancelarReservaAsync(codigoReserva, request, usuario, ct);
        return Ok(ApiResponse<CancelarReservaResponse>.Ok(resultado, "Reserva cancelada exitosamente."));
    }

    // ==========================================================================
    // Endpoint 11 (implicito) - GET /api/v2/booking/reservas/{codigoReserva}/factura
    // ==========================================================================
    [HttpGet("{codigoReserva}/factura")]
    [ProducesResponseType(typeof(ApiResponse<FacturaBookingResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<ActionResult<ApiResponse<FacturaBookingResponse>>> GetFactura(
        [FromRoute] string codigoReserva,
        CancellationToken ct)
    {
        var factura = await _factura.GetFacturaAsync(codigoReserva, ct);
        return Ok(ApiResponse<FacturaBookingResponse>.Ok(factura));
    }
}
