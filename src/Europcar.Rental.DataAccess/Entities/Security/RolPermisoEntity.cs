namespace Europcar.Rental.DataAccess.Entities.Security;

public class RolPermisoEntity
{
    public int IdRolPermiso { get; set; }
    public int IdRol { get; set; }
    public int IdPermiso { get; set; }
    public string EstadoRolPermiso { get; set; } = "ACT";
    public DateTimeOffset FechaRegistroUtc { get; set; } = DateTimeOffset.UtcNow;
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public long RowVersion { get; set; } = 1;

    // Navigation
    public RolEntity Rol { get; set; } = null!;
    public PermisoEntity Permiso { get; set; } = null!;
}
