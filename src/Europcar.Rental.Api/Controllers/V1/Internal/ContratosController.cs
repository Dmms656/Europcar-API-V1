using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.DTOs.Request.Contratos;
using Europcar.Rental.Business.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ContratosController : ControllerBase
{
    private readonly IContratoService _contratoService;

    public ContratosController(IContratoService contratoService)
    {
        _contratoService = contratoService;
    }

    /// <summary>
    /// Obtener todos los contratos.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _contratoService.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener un contrato por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _contratoService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Crear contrato desde una reserva confirmada (apertura de renta).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> Create([FromBody] CrearContratoRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _contratoService.CrearDesdeReservaAsync(request, usuario);
        return CreatedAtAction(nameof(GetById), new { id = result.IdContrato },
            ApiResponse<object>.Ok(result, "Contrato creado exitosamente"));
    }

    /// <summary>
    /// Registrar check-out (entrega del vehículo al cliente).
    /// </summary>
    [HttpPost("checkout")]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _contratoService.RegistrarCheckOutAsync(request, usuario);
        return Ok(ApiResponse<object>.Ok(result, "Check-out registrado exitosamente"));
    }

    /// <summary>
    /// Registrar check-in (devolución del vehículo).
    /// </summary>
    [HttpPost("checkin")]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        var result = await _contratoService.RegistrarCheckInAsync(request, usuario);
        return Ok(ApiResponse<object>.Ok(result, "Check-in registrado exitosamente. Vehículo devuelto."));
    }
}
