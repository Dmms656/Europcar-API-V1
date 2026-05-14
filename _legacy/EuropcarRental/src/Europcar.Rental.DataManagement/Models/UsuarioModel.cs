namespace Europcar.Rental.DataManagement.Models;

public class UsuarioModel
{
    public int IdUsuario { get; set; }
    public Guid UsuarioGuid { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string EstadoUsuario { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public short IntentosFallidos { get; set; }
    public DateTimeOffset? BloqueadoHastaUtc { get; set; }
    public int? IdCliente { get; set; }
    public List<string> Roles { get; set; } = new();
}
