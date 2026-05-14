using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Security;

public class UsuarioRolEntity : BaseEntity
{
    public int IdUsuarioRol { get; set; }
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public string EstadoUsuarioRol { get; set; } = "ACT";
    public bool Activo { get; set; } = true;

    // Navigation
    public UsuarioAppEntity Usuario { get; set; } = null!;
    public RolEntity Rol { get; set; } = null!;
}
