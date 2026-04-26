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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUsuarioDataService usuarioDataService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _usuarioDataService = usuarioDataService;
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

        return new LoginResponse
        {
            Token = token.Token,
            Username = user.Username,
            Correo = user.Correo,
            Roles = user.Roles,
            Expiration = token.Expiration
        };
    }

    public async Task<object> RegisterAsync(RegisterRequest request)
    {
        // Check if username already exists
        if (await _usuarioDataService.ExistsByUsernameAsync(request.Username))
            throw new BusinessException("El nombre de usuario ya está en uso");

        // Hash password
        var (hash, salt) = CreatePasswordHash(request.Password);

        // Create user
        var userId = await _usuarioDataService.CreateUserAsync(
            request.Username, request.Correo, hash, salt, request.IdCliente);

        // Assign CLIENTE role
        await _usuarioDataService.AssignRoleAsync(userId, "CLIENTE");

        return new { userId, username = request.Username, message = "Usuario registrado exitosamente" };
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
