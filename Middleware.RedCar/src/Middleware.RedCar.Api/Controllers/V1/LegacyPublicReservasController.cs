using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Compatibility;
using Middleware.RedCar.Business.Compatibility;
using Middleware.RedCar.Business.DTOs.Reservas;
using Middleware.RedCar.Business.Interfaces;
using BusinessExceptions = global::Middleware.RedCar.Business.Exceptions;

namespace Middleware.RedCar.Api.Controllers.V1;

/// <summary>
/// POST/GET/PATCH bajo /api/v1/reservas (monolito <c>BookingReservasController</c>).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/reservas")]
[Produces("application/json")]
public sealed class LegacyPublicReservasController : ControllerBase
{
    private readonly IReservaOrchestrator _reservas;
    private readonly IFacturaOrchestrator _factura;
    private readonly IValidator<CrearReservaBookingRequest> _crearValidator;
    private readonly IValidator<CancelarReservaRequest> _cancelarValidator;

    public LegacyPublicReservasController(
        IReservaOrchestrator reservas,
        IFacturaOrchestrator factura,
        IValidator<CrearReservaBookingRequest> crearValidator,
        IValidator<CancelarReservaRequest> cancelarValidator)
    {
        _reservas = reservas;
        _factura = factura;
        _crearValidator = crearValidator;
        _cancelarValidator = cancelarValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] LegacyCrearReservaPayload body, CancellationToken ct)
    {
        var contract = LegacyCrearReservaMapper.ToContract(body);
        var vr = await _crearValidator.ValidateAsync(contract, ct);
        if (!vr.IsValid)
        {
            throw new BusinessExceptions.ValidationException(vr.Errors
                .Select(e => new BusinessExceptions.ValidationFailure(e.PropertyName, e.ErrorMessage))
                .ToList());
        }

        var result = await _reservas.CrearReservaAsync(contract, ct);
        return StatusCode(201, LegacyBookingEnvelope.Created(result));
    }

    [HttpGet("{codigoReserva}")]
    public async Task<IActionResult> PorCodigo([FromRoute] string codigoReserva, CancellationToken ct)
    {
        var data = await _reservas.GetReservaAsync(codigoReserva, ct);
        return Ok(LegacyBookingEnvelope.Ok(data));
    }

    [HttpPatch("{codigoReserva}/cancelar")]
    public async Task<IActionResult> Cancelar(
        [FromRoute] string codigoReserva,
        [FromBody] CancelarReservaRequest body,
        CancellationToken ct)
    {
        var vr = await _cancelarValidator.ValidateAsync(body, ct);
        if (!vr.IsValid)
        {
            throw new BusinessExceptions.ValidationException(vr.Errors
                .Select(e => new BusinessExceptions.ValidationFailure(e.PropertyName, e.ErrorMessage))
                .ToList());
        }

        var usuario = User?.Identity?.Name ?? "BOOKING";
        var data = await _reservas.CancelarReservaAsync(codigoReserva, body, usuario, ct);
        return Ok(LegacyBookingEnvelope.Ok(data, "Reserva cancelada exitosamente."));
    }

    [HttpGet("{codigoReserva}/factura")]
    public async Task<IActionResult> Factura([FromRoute] string codigoReserva, CancellationToken ct)
    {
        var data = await _factura.GetFacturaAsync(codigoReserva, ct);
        return Ok(LegacyBookingEnvelope.Ok(data));
    }
}
