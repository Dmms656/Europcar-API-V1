using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.DTOs.Request.Reservas;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;
    private readonly IClienteDataService _clienteDataService;

    public ReservasController(IReservaService reservaService, IClienteDataService clienteDataService)
    {
        _reservaService = reservaService;
        _clienteDataService = clienteDataService;
    }

    /// <summary>
    /// Crear o encontrar un cliente invitado (sin cuenta de usuario).
    /// </summary>
    [HttpPost("guest-client")]
    public async Task<IActionResult> GuestClient([FromBody] GuestClientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Cedula) || string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(ApiResponse<object>.Fail("Cédula y nombre son obligatorios"));

        // Try to find existing client by cédula
        var existing = await _clienteDataService.GetByIdentificacionAsync(request.Cedula.Trim());
        if (existing != null)
        {
            return Ok(ApiResponse<object>.Ok(new
            {
                existing.IdCliente,
                existing.Nombre1,
                existing.Apellido1,
                existing.NumeroIdentificacion,
                existing.Correo,
                esNuevo = false
            }, "Cliente existente encontrado"));
        }

        // Create new client
        var codigo = $"CLT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var newCliente = await _clienteDataService.CreateAsync(new ClienteModel
        {
            CodigoCliente = codigo,
            TipoIdentificacion = "CED",
            NumeroIdentificacion = request.Cedula.Trim(),
            Nombre1 = request.Nombre.Trim(),
            Apellido1 = request.Apellido?.Trim() ?? "",
            Telefono = request.Telefono?.Trim() ?? "",
            Correo = request.Correo?.Trim() ?? "",
            DireccionPrincipal = request.Direccion?.Trim(),
            FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-25))
        });

        return Ok(ApiResponse<object>.Ok(new
        {
            newCliente.IdCliente,
            newCliente.Nombre1,
            newCliente.Apellido1,
            newCliente.NumeroIdentificacion,
            newCliente.Correo,
            esNuevo = true
        }, "Cliente creado exitosamente"));
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
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "GUEST";
        var result = await _reservaService.ConfirmarAsync(id, usuario, request?.Monto, request?.ReferenciaExterna);
        return Ok(ApiResponse<object>.Ok(result, "Reserva confirmada exitosamente"));
    }

    /// <summary>
    /// Cancelar una reserva.
    /// </summary>
    [HttpPut("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarReservaRequest request)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "GUEST";
        var result = await _reservaService.CancelarAsync(id, request.Motivo, usuario);
        return Ok(ApiResponse<object>.Ok(result, "Reserva cancelada exitosamente"));
    }
}

public class GuestClientRequest
{
    public string Cedula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Apellido { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}
