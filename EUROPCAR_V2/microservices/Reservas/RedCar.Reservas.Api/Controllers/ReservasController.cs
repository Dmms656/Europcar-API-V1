using Microsoft.AspNetCore.Mvc;
using RedCar.Reservas.Api.Contracts;
using RedCar.Reservas.Api.Services;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Reservas.Api.Controllers;

[ApiController]
[Route("api/v1/reservas")]
public sealed class ReservasController : ControllerBase
{
    private readonly ReservasReadService _read;

    public ReservasController(ReservasReadService read) => _read = read;

    [HttpGet("disponibilidad")]
    public async Task<ActionResult<ApiResponse<DisponibilidadDto>>> Disponibilidad(
        [FromQuery] int idVehiculo,
        [FromQuery] int idLocalizacion,
        [FromQuery] DateTimeOffset fechaRecogida,
        [FromQuery] DateTimeOffset fechaDevolucion,
        CancellationToken ct)
    {
        var dto = await _read.VerificarDisponibilidadAsync(idVehiculo, idLocalizacion, fechaRecogida, fechaDevolucion, ct);
        return Ok(ApiResponse<DisponibilidadDto>.Ok(dto, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{codigoReserva}/factura")]
    public async Task<ActionResult<ApiResponse<FacturaDto>>> Factura(string codigoReserva, CancellationToken ct)
    {
        var dto = await _read.GetFacturaAsync(codigoReserva, ct);
        if (dto is null)
        {
            return NotFound(ApiResponse<FacturaDto>.Fail(404, "Factura no encontrada.", HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<FacturaDto>.Ok(dto, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{codigoReserva}")]
    public async Task<ActionResult<ApiResponse<ReservaDto>>> GetByCodigo(string codigoReserva, CancellationToken ct)
    {
        var dto = await _read.GetReservaAsync(codigoReserva, ct);
        if (dto is null)
        {
            return NotFound(ApiResponse<ReservaDto>.Fail(404, "Reserva no encontrada.", HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<ReservaDto>.Ok(dto, traceId: HttpContext.TraceIdentifier));
    }
}
