using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
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
    /// Iniciar sesión y obtener token JWT.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<object>.Ok(result, "Login exitoso"));
    }

    /// <summary>
    /// Registrar un nuevo usuario (cliente).
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<object>.Ok(result, "Registro exitoso"));
    }

    /// <summary>
    /// Validar si una cédula/identificación ya existe.
    /// </summary>
    [HttpGet("cedula-exists")]
    public async Task<IActionResult> CedulaExists([FromQuery] string cedula)
    {
        if (string.IsNullOrWhiteSpace(cedula))
            return BadRequest(ApiResponse<object>.Fail("La cédula es requerida"));

        var existing = await _clienteDataService.GetByIdentificacionAsync(cedula.Trim());
        return Ok(ApiResponse<object>.Ok(new { exists = existing != null }));
    }
}
