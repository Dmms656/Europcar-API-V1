using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Compatibility;
using Middleware.RedCar.DataAccess.Clients.Interfaces;
using RedCar.Shared.Contracts.Common;

namespace Middleware.RedCar.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/Clientes")]
[Produces("application/json")]
public sealed class LegacyClientesController : ControllerBase
{
    private readonly IClientesClient _clientes;

    public LegacyClientesController(IClientesClient clientes) => _clientes = clientes;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _clientes.ListAllAsync(ct: ct) ?? Array.Empty<ClienteListItemDto>();
        return Ok(ApiResponse<object>.Ok(items.Select(LegacyAdminDtoMapper.ToCliente).ToList(), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var c = await _clientes.GetByIdAsync(id, ct);
        if (c is null)
            return NotFound(ApiResponse<object>.Fail(404, "Cliente no encontrado", HttpContext.TraceIdentifier));

        var listItem = new ClienteListItemDto(
            c.IdCliente, Guid.Empty, string.Empty, c.TipoIdentificacion, c.NumeroIdentificacion,
            $"{c.Nombres} {c.Apellidos}".Trim(), c.Nombres, null, c.Apellidos, null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)), c.Telefono, c.Correo, null, "ACT", 0);

        return Ok(ApiResponse<object>.Ok(LegacyAdminDtoMapper.ToCliente(listItem), traceId: HttpContext.TraceIdentifier));
    }

    [HttpPost]
    [HttpPut("{id:int}")]
    [HttpDelete("{id:int}")]
    public IActionResult NotImplementedWrite()
        => StatusCode(501, ApiResponse<object>.Fail(501, "CRUD de clientes no implementado; use registro web o upsert en MS Clientes.", HttpContext.TraceIdentifier));
}
