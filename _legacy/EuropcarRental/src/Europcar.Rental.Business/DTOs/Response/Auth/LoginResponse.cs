namespace Europcar.Rental.Business.DTOs.Response.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime Expiration { get; set; }
    public int? IdCliente { get; set; }
    public string? NombreCompleto { get; set; }
}
