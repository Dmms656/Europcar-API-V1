using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Extensions;
using Middleware.RedCar.DataAccess.Clients.Interfaces;
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
    private readonly IClientesClient _clientes;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, IClientesClient clientes, ILogger<AuthController> logger)
    {
        _auth = auth;
        _clientes = clientes;
        _logger = logger;
    }

    /// <summary>Login: cookie HttpOnly + token en JSON (solo memoria en el SPA, no localStorage).</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var data = await _auth.LoginAsync(request, ct);
            AuthCookieExtensions.SetAuthCookie(Request, Response, data.Token, data.Expiration);

            return Ok(ApiResponse<object>.Ok(new
            {
                data.Username,
                data.Correo,
                data.Roles,
                data.Expiration,
                data.IdCliente,
                data.NombreCompleto,
                data.Token
            }, "Login exitoso", HttpContext.TraceIdentifier));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message, HttpContext.TraceIdentifier));
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Login: error Npgsql (Inner={Inner})", ex.InnerException?.Message);
            return StatusCode(500, ApiResponse<object>.Fail(500,
                "Error al conectar con la base de datos. Si usas Supabase pooler (6543), revisa la cadena de conexión y los logs del servidor.",
                HttpContext.TraceIdentifier));
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Login: error de lectura en el stream (Inner={Inner})", ex.InnerException?.Message);
            return StatusCode(500, ApiResponse<object>.Fail(500,
                "Error al conectar con la base de datos. Si usas Supabase pooler (6543), revisa la cadena de conexión y los logs del servidor.",
                HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login: error no controlado.");
            return StatusCode(500, ApiResponse<object>.Fail(500, ex.Message, HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        try
        {
            var idCliente = await ResolveIdClienteForRegisterAsync(request, ct);
            if (idCliente.HasValue)
                request.IdCliente = idCliente;

            var data = await _auth.RegisterAsync(request, ct);
            return Ok(ApiResponse<object>.Ok(data, "Registro exitoso", HttpContext.TraceIdentifier));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message, HttpContext.TraceIdentifier));
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Login: error Npgsql (Inner={Inner})", ex.InnerException?.Message);
            return StatusCode(500, ApiResponse<object>.Fail(500,
                "Error al conectar con la base de datos. Si usas Supabase pooler (6543), revisa la cadena de conexión y los logs del servidor.",
                HttpContext.TraceIdentifier));
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Login: error de lectura en el stream (Inner={Inner})", ex.InnerException?.Message);
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

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        AuthCookieExtensions.ClearAuthCookie(Request, Response);
        return Ok(ApiResponse<object>.Ok(new { }, "Sesión cerrada", HttpContext.TraceIdentifier));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("unique_name");
        var correo = User.FindFirstValue(ClaimTypes.Email);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var idClienteClaim = User.FindFirstValue("idCliente");
        int? idCliente = int.TryParse(idClienteClaim, out var id) ? id : null;

        string? nombreCompleto = User.FindFirstValue("nombre_completo");
        string? numeroIdentificacion = null;
        string? nombres = null;
        string? apellidos = null;
        string? telefono = null;

        if (idCliente.HasValue)
        {
            try
            {
                var perfil = await _clientes.GetByIdAsync(idCliente.Value, ct);
                if (perfil is not null)
                {
                    nombres = perfil.Nombres;
                    apellidos = perfil.Apellidos;
                    numeroIdentificacion = perfil.NumeroIdentificacion;
                    telefono = perfil.Telefono;
                    nombreCompleto ??= $"{perfil.Nombres} {perfil.Apellidos}".Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo cargar perfil de cliente {IdCliente} en /me", idCliente);
            }
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            username,
            correo,
            roles,
            idCliente,
            nombreCompleto,
            numeroIdentificacion,
            nombres,
            apellidos,
            telefono
        }, "OK", HttpContext.TraceIdentifier));
    }

    [HttpGet("cedula-exists")]
    [AllowAnonymous]
    public async Task<IActionResult> CedulaExists([FromQuery] string cedula, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cedula))
            return BadRequest(ApiResponse<object>.Fail(400, "La cédula es requerida", HttpContext.TraceIdentifier));

        ClienteDetalleDto? cliente = null;
        try
        {
            cliente = await _clientes.GetByIdentificacionAsync(cedula.Trim(), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "cedula-exists: no se pudo consultar MS Clientes");
        }

        return Ok(ApiResponse<object>.Ok(
            new { exists = cliente is not null },
            "OK",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Crea o vincula el registro en MS.Clientes antes de crear el usuario en seguridad.
    /// </summary>
    private async Task<int?> ResolveIdClienteForRegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        if (request.IdCliente.HasValue)
            return request.IdCliente;

        var cedula = (request.Cedula ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(cedula))
            return null;

        var vincularExistente = string.IsNullOrWhiteSpace(request.Nombre);

        if (vincularExistente)
        {
            var existente = await _clientes.GetByIdentificacionAsync(cedula, ct);
            if (existente is null)
            {
                throw new InvalidOperationException(
                    $"No se encontró un cliente con la identificación '{cedula}'. " +
                    "Verifica la cédula o regístrate como «Nuevo Cliente».");
            }

            return existente.IdCliente;
        }

        var upsert = new ClienteUpsertRequest(
            Nombres: request.Nombre!.Trim(),
            Apellidos: string.IsNullOrWhiteSpace(request.Apellido) ? "N/A" : request.Apellido.Trim(),
            TipoIdentificacion: "CEDULA",
            NumeroIdentificacion: cedula,
            Correo: request.Correo.Trim(),
            Telefono: string.IsNullOrWhiteSpace(request.Telefono) ? "0999999999" : request.Telefono.Trim());

        var creado = await _clientes.UpsertClienteAsync(upsert, ct)
            ?? throw new InvalidOperationException("No se pudo registrar el cliente en el sistema.");

        return creado.IdCliente;
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
