using Europcar.Rental.DataAccess.Entities.Common;
using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Entities.Security;

public class UsuarioAppEntity : BaseEntity
{
    public int IdUsuario { get; set; }
    public Guid UsuarioGuid { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string? PasswordHint { get; set; }
    public bool RequiereCambioPassword { get; set; } = true;
    public string EstadoUsuario { get; set; } = "ACT";
    public bool Activo { get; set; } = true;
    public short IntentosFallidos { get; set; } = 0;
    public DateTimeOffset? BloqueadoHastaUtc { get; set; }
    public DateTimeOffset? UltimoLoginUtc { get; set; }
    public int? IdCliente { get; set; }

    // Navigation
    public ClienteEntity? Cliente { get; set; }
    public ICollection<UsuarioRolEntity> UsuariosRoles { get; set; } = new List<UsuarioRolEntity>();
}
