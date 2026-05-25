using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using RedCar.Reservas.Api.Contracts;
using RedCar.Reservas.Api.Extensions;
using RedCar.Reservas.Api.Mapping;
using RedCar.Reservas.Api.Services;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Reservas.Api.Controllers;

[ApiController]
[Route("api/v1/reservas")]
public sealed class ReservasController : ControllerBase
{
    private readonly ReservasReadService _read;
    private readonly ReservasWriteService _write;

    public ReservasController(ReservasReadService read, ReservasWriteService write)
    {
        _read = read;
        _write = write;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CrearReservaRestResponse>>> Crear(
        [FromBody] CrearReservaRestRequest request,
        CancellationToken ct)
    {
        try
        {
            var proto = ReservasWriteMapper.ToProto(request);
            var result = await _write.CrearReservaAsync(proto, ct);
            return Ok(ApiResponse<CrearReservaRestResponse>.Ok(
                ReservasWriteMapper.ToRest(result), traceId: HttpContext.TraceIdentifier));
        }
        catch (RpcException ex)
        {
            return RpcExceptionMapper.ToActionResult<CrearReservaRestResponse>(ex, HttpContext);
        }
    }

    [HttpPatch("{codigoReserva}/cancelar")]
    public async Task<ActionResult<ApiResponse<CancelarReservaRestResponse>>> Cancelar(
        [FromRoute] string codigoReserva,
        [FromBody] CancelarReservaRestRequest request,
        CancellationToken ct)
    {
        try
        {
            var proto = ReservasWriteMapper.ToProto(codigoReserva, request);
            var result = await _write.CancelarReservaAsync(proto, ct);
            return Ok(ApiResponse<CancelarReservaRestResponse>.Ok(
                ReservasWriteMapper.ToRest(result), traceId: HttpContext.TraceIdentifier));
        }
        catch (RpcException ex)
        {
            return RpcExceptionMapper.ToActionResult<CancelarReservaRestResponse>(ex, HttpContext);
        }
    }

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

    [HttpGet("cliente/{idCliente:int}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClienteReservaListItemDto>>>> ListByCliente(
        [FromRoute] int idCliente,
        CancellationToken ct)
    {
        var items = await _read.ListByClienteAsync(idCliente, ct);
        return Ok(ApiResponse<IReadOnlyList<ClienteReservaListItemDto>>.Ok(items, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{idReserva:int}/cancelar")]
    public async Task<ActionResult<ApiResponse<CancelarReservaRestResponse>>> CancelarById(
        [FromRoute] int idReserva,
        [FromBody] CancelarReservaRestRequest request,
        CancellationToken ct)
    {
        var codigo = await _read.GetCodigoReservaByIdAsync(idReserva, ct);
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return NotFound(ApiResponse<CancelarReservaRestResponse>.Fail(404, "Reserva no encontrada.", HttpContext.TraceIdentifier));
        }

        try
        {
            var proto = ReservasWriteMapper.ToProto(codigo, request);
            var result = await _write.CancelarReservaAsync(proto, ct);
            return Ok(ApiResponse<CancelarReservaRestResponse>.Ok(
                ReservasWriteMapper.ToRest(result), traceId: HttpContext.TraceIdentifier));
        }
        catch (RpcException ex)
        {
            return RpcExceptionMapper.ToActionResult<CancelarReservaRestResponse>(ex, HttpContext);
        }
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
