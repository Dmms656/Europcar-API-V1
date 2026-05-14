using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.DataManagement.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class FacturasController : ControllerBase
{
    private readonly IFacturaDataService _facturaDataService;
    private readonly IUsuarioDataService _usuarioDataService;

    public FacturasController(
        IFacturaDataService facturaDataService,
        IUsuarioDataService usuarioDataService)
    {
        _facturaDataService = facturaDataService;
        _usuarioDataService = usuarioDataService;
    }

    /// <summary>
    /// Obtener facturas del cliente autenticado (solo lectura).
    /// </summary>
    [HttpGet("mis-facturas")]
    [Authorize(Roles = "CLIENTE_WEB")]
    public async Task<IActionResult> GetMyFacturas()
    {
        var username = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized(ApiResponse<object>.Fail("Usuario no autenticado"));

        var usuario = await _usuarioDataService.GetByUsernameAsync(username);
        if (usuario?.IdCliente == null)
            return BadRequest(ApiResponse<object>.Fail("El usuario no tiene un cliente asociado"));

        var result = await _facturaDataService.GetByClienteIdAsync(usuario.IdCliente.Value);
        return Ok(ApiResponse<object>.Ok(result));
    }
}
