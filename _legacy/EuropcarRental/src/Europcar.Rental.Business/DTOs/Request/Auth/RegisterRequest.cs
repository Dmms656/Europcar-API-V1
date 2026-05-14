namespace Europcar.Rental.Business.DTOs.Request.Auth;

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    // Optional: Link to existing client
    public int? IdCliente { get; set; }
    // For new client creation
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}
