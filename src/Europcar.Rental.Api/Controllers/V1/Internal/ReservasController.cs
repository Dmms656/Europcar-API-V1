using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.DTOs.Request.Reservas;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

/// <summary>
/// Gestión interna de reservas (back-office).
/// Se monta en /api/v1/admin/reservas para liberar la ruta pública /api/v1/reservas
/// usada por el contrato Booking.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/reservas")]
[Authorize]
public class ReservasController : ControllerBase
{
    private const string AdminRoles = "ADMIN,AGENTE_POS";

    private readonly IReservaService _reservaService;
    private readonly IReservaDataService _reservaDataService;
    private readonly IClienteDataService _clienteDataService;

    public ReservasController(
        IReservaService reservaService,
        IReservaDataService reservaDataService,
        IClienteDataService clienteDataService)
    {
        _reservaService = reservaService;
        _reservaDataService = reservaDataService;
        _clienteDataService = clienteDataService;
    }

    /// <summary>
    /// Crear o encontrar un cliente invitado (sin cuenta de usuario).
    /// </summary>
    [HttpPost("guest-client")]
    [AllowAnonymous]
    public async Task<IActionResult> GuestClient([FromBody] GuestClientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Cedula) || string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(ApiResponse<object>.Fail("Cédula y nombre son obligatorios"));

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
        EnsureMismoClienteOAdmin(idCliente);
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
    /// Cancelar una reserva. Para clientes (rol CLIENTE / sin rol admin) sólo se permite
    /// cancelar reservas propias y cuya fecha de recogida sea futura. Las pasadas quedan
    /// bloqueadas. Pagos y facturas asociados se anulan automáticamente.
    /// </summary>
    [HttpPut("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarReservaRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Motivo))
            throw new BusinessException("El motivo de cancelación es requerido");

        await EnsurePuedeCancelarAsync(id);

        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "GUEST";
        var result = await _reservaService.CancelarAsync(id, request.Motivo, usuario);
        return Ok(ApiResponse<object>.Ok(result, "Reserva cancelada exitosamente"));
    }

    // ============================================================
    // Helpers de autorización
    // ============================================================

    /// <summary>
    /// Garantiza que un usuario sin rol administrativo sólo pueda actuar
    /// sobre las reservas asociadas a su propio cliente.
    /// </summary>
    private async Task EnsurePuedeCancelarAsync(int idReserva)
    {
        if (EsAdminOAgente()) return;

        var reserva = await _reservaDataService.GetByIdAsync(idReserva)
            ?? throw new NotFoundException($"Reserva con ID {idReserva} no encontrada");

        var idClienteUsuario = GetIdClienteClaim()
            ?? throw new ForbiddenException("Tu usuario no está asociado a un cliente");

        if (reserva.IdCliente != idClienteUsuario)
            throw new ForbiddenException("No puedes cancelar reservas que no te pertenecen");
    }

    private void EnsureMismoClienteOAdmin(int idCliente)
    {
        if (EsAdminOAgente()) return;

        var idClienteUsuario = GetIdClienteClaim();
        if (idClienteUsuario == null || idClienteUsuario.Value != idCliente)
            throw new ForbiddenException("No puedes consultar reservas de otros clientes");
    }

    private bool EsAdminOAgente()
    {
        return AdminRoles.Split(',').Any(r => User.IsInRole(r));
    }

    private int? GetIdClienteClaim()
    {
        var raw = User.FindFirstValue("idCliente");
        return int.TryParse(raw, out var id) ? id : null;
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
