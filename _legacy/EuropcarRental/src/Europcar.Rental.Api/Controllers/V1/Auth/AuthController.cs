using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Europcar.Rental.Api.Extensions;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.DTOs.Request.Auth;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IClienteDataService _clienteDataService;

    public AuthController(IAuthService authService, IClienteDataService clienteDataService)
    {
        _authService = authService;
        _clienteDataService = clienteDataService;
    }

    /// <summary>
    /// Iniciar sesión. El JWT se entrega en cookie HttpOnly (no en el cuerpo JSON).
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        AuthCookieExtensions.SetAuthCookie(Response, result.Token, result.Expiration, Request.IsHttps);

        return Ok(ApiResponse<object>.Ok(new
        {
            result.Username,
            result.Correo,
            result.Roles,
            result.Expiration,
            result.IdCliente,
            result.NombreCompleto
        }, "Login exitoso"));
    }

    /// <summary>
    /// Registrar un nuevo usuario (cliente). JWT en cookie HttpOnly.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<object>.Ok(result, "Registro exitoso"));
    }

    /// <summary>
    /// Cerrar sesión (elimina la cookie de autenticación).
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        AuthCookieExtensions.ClearAuthCookie(Response);
        return Ok(ApiResponse<object>.Ok(new { }, "Sesión cerrada"));
    }

    /// <summary>
    /// Perfil de la sesión actual (restaura el estado del frontend sin localStorage del token).
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("unique_name");
        var correo = User.FindFirstValue(ClaimTypes.Email);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var idClienteClaim = User.FindFirstValue("idCliente");
        int? idCliente = int.TryParse(idClienteClaim, out var id) ? id : null;

        return Ok(ApiResponse<object>.Ok(new
        {
            username,
            correo,
            roles,
            idCliente,
            nombreCompleto = User.FindFirstValue("nombre_completo")
        }));
    }

    /// <summary>
    /// Validar si una cédula/identificación ya existe (solo booleano, sin PII).
    /// </summary>
    [HttpGet("cedula-exists")]
    [EnableRateLimiting("guest-client")]
    public async Task<IActionResult> CedulaExists([FromQuery] string cedula)
    {
        if (string.IsNullOrWhiteSpace(cedula))
            return BadRequest(ApiResponse<object>.Fail("La cédula es requerida"));

        var existing = await _clienteDataService.GetByIdentificacionAsync(cedula.Trim());
        return Ok(ApiResponse<object>.Ok(new { exists = existing != null }));
    }
}
