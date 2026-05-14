using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using RedCar.Seguridad.Business.Auth;
using RedCar.Shared.Contracts.Common;

namespace Middleware.RedCar.Api.Controllers.V1;

/// <summary>Rutas <c>/api/v1/Auth/*</c> (SPA); auth embebido sin MS Seguridad separado.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/Auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, ILogger<AuthController> logger)
    {
        _auth = auth;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var data = await _auth.LoginAsync(request, ct);
            return Ok(ApiResponse<LoginResponseDto>.Ok(data, "Login exitoso", HttpContext.TraceIdentifier));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse<LoginResponseDto>.Fail(400, ex.Message, HttpContext.TraceIdentifier));
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Login: error Npgsql (Inner={Inner})", ex.InnerException?.Message);
            return StatusCode(500, ApiResponse<LoginResponseDto>.Fail(500,
                "Error al conectar con la base de datos. Si usas Supabase pooler (6543), revisa la cadena de conexión y los logs del servidor.",
                HttpContext.TraceIdentifier));
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Login: error de lectura en el stream (Inner={Inner})", ex.InnerException?.Message);
            return StatusCode(500, ApiResponse<LoginResponseDto>.Fail(500,
                "Error al conectar con la base de datos. Si usas Supabase pooler (6543), revisa la cadena de conexión y los logs del servidor.",
                HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login: error no controlado.");
            return StatusCode(500, ApiResponse<LoginResponseDto>.Fail(500, ex.Message, HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        try
        {
            var data = await _auth.RegisterAsync(request, ct);
            return Ok(ApiResponse<object>.Ok(data, "Registro exitoso", HttpContext.TraceIdentifier));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message, HttpContext.TraceIdentifier));
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Register: error Npgsql (Inner={Inner})", ex.InnerException?.Message);
            return StatusCode(500, ApiResponse<object>.Fail(500,
                "Error al conectar con la base de datos. Si usas Supabase pooler (6543), revisa la cadena de conexión y los logs del servidor.",
                HttpContext.TraceIdentifier));
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Register: error de lectura en el stream (Inner={Inner})", ex.InnerException?.Message);
            return StatusCode(500, ApiResponse<object>.Fail(500,
                "Error al conectar con la base de datos. Si usas Supabase pooler (6543), revisa la cadena de conexión y los logs del servidor.",
                HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register: error no controlado.");
            return StatusCode(500, ApiResponse<object>.Fail(500, ex.Message, HttpContext.TraceIdentifier));
        }
    }

    [HttpGet("cedula-exists")]
    [AllowAnonymous]
    public IActionResult CedulaExists([FromQuery] string cedula)
    {
        if (string.IsNullOrWhiteSpace(cedula))
            return BadRequest(ApiResponse<object>.Fail(400, "La cédula es requerida", HttpContext.TraceIdentifier));
        return Ok(ApiResponse<object>.Ok(new { exists = false }, "OK", HttpContext.TraceIdentifier));
    }

    [HttpPut("profile")]
    [Authorize]
    public IActionResult ProfileNotImplemented() =>
        StatusCode(501, new
        {
            success = false,
            statusCode = 501,
            message = "Actualización de perfil aún no está disponible.",
            data = (object?)null,
            traceId = HttpContext.TraceIdentifier
        });

    [HttpPut("change-password")]
    [Authorize]
    public IActionResult ChangePasswordNotImplemented() =>
        StatusCode(501, new
        {
            success = false,
            statusCode = 501,
            message = "Cambio de contraseña aún no está disponible.",
            data = (object?)null,
            traceId = HttpContext.TraceIdentifier
        });
}
