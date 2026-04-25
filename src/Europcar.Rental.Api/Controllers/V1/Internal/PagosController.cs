using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.DTOs.Request.Pagos;
using Europcar.Rental.Business.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class PagosController : ControllerBase
{
    private readonly IPagoService _pagoService;

    public PagosController(IPagoService pagoService)
    {
        _pagoService = pagoService;
    }

    /// <summary>
    /// Obtener un pago por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _pagoService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener pagos por reserva.
    /// </summary>
    [HttpGet("reserva/{idReserva:int}")]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> GetByReserva(int idReserva)
    {
        var result = await _pagoService.GetByReservaIdAsync(idReserva);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Registrar un pago.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> Create([FromBody] CrearPagoRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _pagoService.CreateAsync(request, usuario);
        return CreatedAtAction(nameof(GetById), new { id = result.IdPago },
            ApiResponse<object>.Ok(result, "Pago registrado exitosamente"));
    }
}
