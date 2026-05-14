using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Security;

public class RolEntity : BaseEntity
{
    public int IdRol { get; set; }
    public Guid RolGuid { get; set; }
    public string NombreRol { get; set; } = string.Empty;
    public string? DescripcionRol { get; set; }
    public bool EsSistema { get; set; } = false;
    public string EstadoRol { get; set; } = "ACT";

    // Navigation
    public ICollection<UsuarioRolEntity> UsuariosRoles { get; set; } = new List<UsuarioRolEntity>();
}
