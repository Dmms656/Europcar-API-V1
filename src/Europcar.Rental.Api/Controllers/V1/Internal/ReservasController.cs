using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.DTOs.Request.Reservas;
using Europcar.Rental.Business.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    /// <summary>
    /// Crear una nueva reserva.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearReservaRequest request)
    {
        var result = await _reservaService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByCodigo), new { codigo = result.CodigoReserva }, 
            ApiResponse<object>.Ok(result, "Reserva creada exitosamente"));
    }

    /// <summary>
    /// Obtener una reserva por su código.
    /// </summary>
    [HttpGet("{codigo}")]
    public async Task<IActionResult> GetByCodigo(string codigo)
    {
        var result = await _reservaService.GetByCodigoAsync(codigo);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener reservas de un cliente.
    /// </summary>
    [HttpGet("cliente/{idCliente:int}")]
    public async Task<IActionResult> GetByCliente(int idCliente)
    {
        var result = await _reservaService.GetByClienteIdAsync(idCliente);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Confirmar una reserva pendiente y registrar el pago + factura.
    /// </summary>
    [HttpPut("{id:int}/confirmar")]
    public async Task<IActionResult> Confirmar(int id, [FromBody] ConfirmarReservaRequest? request = null)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _reservaService.ConfirmarAsync(id, usuario, request?.Monto, request?.ReferenciaExterna);
        return Ok(ApiResponse<object>.Ok(result, "Reserva confirmada exitosamente"));
    }

    /// <summary>
    /// Cancelar una reserva.
    /// </summary>
    [HttpPut("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarReservaRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _reservaService.CancelarAsync(id, request.Motivo, usuario);
        return Ok(ApiResponse<object>.Ok(result, "Reserva cancelada exitosamente"));
    }
}
