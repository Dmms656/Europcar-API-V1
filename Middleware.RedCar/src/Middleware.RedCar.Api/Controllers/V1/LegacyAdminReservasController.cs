using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.DataAccess.Clients;
using Middleware.RedCar.DataAccess.Clients.Interfaces;
using RedCar.Shared.Contracts.Common;

namespace Middleware.RedCar.Api.Controllers.V1;

/// <summary>
/// Portal cliente y back-office: <c>GET /api/v1/admin/Reservas/cliente/{id}</c>, cancelación por id.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/admin/Reservas")]
[Produces("application/json")]
public sealed class LegacyAdminReservasController : ControllerBase
{
    private static readonly string[] AdminRoles = ["ADMIN", "AGENTE", "AGENTE_POS"];

    private readonly IReservasClient _reservas;

    public LegacyAdminReservasController(IReservasClient reservas) => _reservas = reservas;

    [HttpGet("cliente/{idCliente:int}")]
    public async Task<IActionResult> GetByCliente([FromRoute] int idCliente, CancellationToken ct)
    {
        if (!EnsureMismoClienteOAdmin(idCliente))
            return StatusCode(403, ApiResponse<object>.Fail(403, "No puedes consultar reservas de otros clientes"));

        var items = await _reservas.ListByClienteAsync(idCliente, ct) ?? Array.Empty<ClienteReservaListItemDto>();
        return Ok(ApiResponse<IReadOnlyList<ClienteReservaListItemDto>>.Ok(items, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar([FromRoute] int id, [FromBody] LegacyCancelarReservaRequest? request, CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Motivo))
            return BadRequest(ApiResponse<object>.Fail(400, "El motivo de cancelación es requerido"));

        if (!await EnsurePuedeCancelarAsync(id, ct))
            return StatusCode(403, ApiResponse<object>.Fail(403, "No puedes cancelar esta reserva"));

        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("unique_name") ?? "PORTAL";
        try
        {
            var result = await _reservas.CancelarByIdAsync(id, request.Motivo.Trim(), usuario, ct);
            return Ok(ApiResponse<CancelarReservaWriteResult>.Ok(result, "Reserva cancelada exitosamente", HttpContext.TraceIdentifier));
        }
        catch (MicroserviceClientException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound(ApiResponse<object>.Fail(404, "Reserva no encontrada"));
        }
    }

    private bool EnsureMismoClienteOAdmin(int idCliente)
    {
        if (EsAdminOAgente()) return true;
        var idUsuario = GetIdClienteClaim();
        return idUsuario.HasValue && idUsuario.Value == idCliente;
    }

    private async Task<bool> EnsurePuedeCancelarAsync(int idReserva, CancellationToken ct)
    {
        if (EsAdminOAgente()) return true;

        var idCliente = GetIdClienteClaim();
        if (!idCliente.HasValue) return false;

        var items = await _reservas.ListByClienteAsync(idCliente.Value, ct);
        return items?.Any(r => r.IdReserva == idReserva) == true;
    }

    private bool EsAdminOAgente() => AdminRoles.Any(User.IsInRole);

    private int? GetIdClienteClaim()
    {
        var raw = User.FindFirstValue("idCliente");
        return int.TryParse(raw, out var id) ? id : null;
    }
}

public sealed class LegacyCancelarReservaRequest
{
    public string Motivo { get; set; } = string.Empty;
}
