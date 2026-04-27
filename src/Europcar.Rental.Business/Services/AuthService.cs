using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Europcar.Rental.Business.DTOs.Request.Auth;
using Europcar.Rental.Business.DTOs.Response.Auth;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Common;
using Europcar.Rental.DataManagement.Interfaces;

namespace Europcar.Rental.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioDataService _usuarioDataService;
    private readonly IClienteDataService _clienteDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUsuarioDataService usuarioDataService,
        IClienteDataService clienteDataService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _usuarioDataService = usuarioDataService;
        _clienteDataService = clienteDataService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _usuarioDataService.GetByUsernameAsync(request.Username)
            ?? throw new UnauthorizedException("Credenciales inválidas");

        if (!user.Activo || user.EstadoUsuario != "ACT")
            throw new UnauthorizedException("Usuario inactivo o bloqueado");

        if (user.BloqueadoHastaUtc.HasValue && user.BloqueadoHastaUtc > DateTimeOffset.UtcNow)
            throw new UnauthorizedException($"Usuario bloqueado hasta {user.BloqueadoHastaUtc:yyyy-MM-dd HH:mm:ss} UTC");

        if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            throw new UnauthorizedException("Credenciales inválidas");

        await _usuarioDataService.UpdateUltimoLoginAsync(user.IdUsuario);
        await _unitOfWork.SaveChangesAsync();

        var token = GenerateJwtToken(user.IdUsuario, user.Username, user.Correo, user.Roles);

        // Lookup client name if user is linked to a client
        string? nombreCompleto = null;
        if (user.IdCliente.HasValue)
        {
            var cliente = await _clienteDataService.GetByIdAsync(user.IdCliente.Value);
            if (cliente != null)
                nombreCompleto = $"{cliente.Nombre1} {cliente.Apellido1}".Trim();
        }

        return new LoginResponse
        {
            Token = token.Token,
            Username = user.Username,
            Correo = user.Correo,
            Roles = user.Roles,
            Expiration = token.Expiration,
            IdCliente = user.IdCliente,
            NombreCompleto = nombreCompleto
        };
    }

    public async Task<object> RegisterAsync(RegisterRequest request)
    {
        // Check if username already exists
        if (await _usuarioDataService.ExistsByUsernameAsync(request.Username))
            throw new BusinessException("El nombre de usuario ya está en uso");

        // Hash password
        var (hash, salt) = CreatePasswordHash(request.Password);

        int? idCliente = request.IdCliente;

        // Mode 1: Link to existing client by cedula/ID
        if (!idCliente.HasValue && !string.IsNullOrEmpty(request.Cedula) && string.IsNullOrEmpty(request.Nombre))
        {
            // Try to find existing client by identification number
            var existingCliente = await _clienteDataService.GetByIdentificacionAsync(request.Cedula);
            if (existingCliente == null)
            {
                // Try parsing as numeric ID
                if (int.TryParse(request.Cedula, out var parsedId))
                {
                    existingCliente = await _clienteDataService.GetByIdAsync(parsedId);
                }
            }
            if (existingCliente == null)
                throw new BusinessException($"No se encontró un cliente con la identificación '{request.Cedula}'. Verifica el dato o regístrate como nuevo cliente.");

            idCliente = existingCliente.IdCliente;
        }

        // Mode 2: Create new client from registration data
        if (!idCliente.HasValue && !string.IsNullOrEmpty(request.Nombre))
        {
            var codigoCliente = $"CLT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var newCliente = await _clienteDataService.CreateAsync(new DataManagement.Models.ClienteModel
            {
                CodigoCliente = codigoCliente,
                TipoIdentificacion = "CED",
                NumeroIdentificacion = request.Cedula ?? "",
                Nombre1 = request.Nombre ?? "",
                Apellido1 = request.Apellido ?? "",
                Telefono = request.Telefono ?? "",
                Correo = request.Correo,
                DireccionPrincipal = request.Direccion,
                FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-25))
            });
            idCliente = newCliente.IdCliente;
        }

        // Create user linked to client
        var userId = await _usuarioDataService.CreateUserAsync(
            request.Username, request.Correo, hash, salt, idCliente);

        // Assign CLIENTE role
        await _usuarioDataService.AssignRoleAsync(userId, "CLIENTE_WEB");

        return new { userId, username = request.Username, idCliente, message = "Usuario registrado exitosamente" };
    }

    private static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        using var hmac = new HMACSHA512(saltBytes);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(computedHash) == storedHash;
    }

    /// <summary>
    /// Genera hash y salt para una contraseña nueva (usado en seeding).
    /// </summary>
    public static (string hash, string salt) CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = Convert.ToBase64String(hmac.Key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        return (hash, salt);
    }

    private (string Token, DateTime Expiration) GenerateJwtToken(int userId, string username, string correo, List<string> roles)
    {
        var jwtSection = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurado");
        var issuer = jwtSection["Issuer"] ?? "Europcar.Rental.Api";
        var audience = jwtSection["Audience"] ?? "Europcar.Rental.Client";
        var expirationMinutes = int.Parse(jwtSection["ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, correo),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }
}
