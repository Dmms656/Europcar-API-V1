namespace RedCar.Seguridad.Business.Auth;

public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public int? IdCliente { get; set; }
}

/// <summary>Misma forma que el monolito para el SPA (LoginPage).</summary>
public sealed class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime Expiration { get; set; }
    public int? IdCliente { get; set; }
    public string? NombreCompleto { get; set; }
}
