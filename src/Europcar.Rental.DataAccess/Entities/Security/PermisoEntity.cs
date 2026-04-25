namespace Europcar.Rental.DataAccess.Entities.Security;

public class PermisoEntity
{
    public int IdPermiso { get; set; }
    public Guid PermisoGuid { get; set; }
    public string CodigoPermiso { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string? DescripcionPermiso { get; set; }
    public string EstadoPermiso { get; set; } = "ACT";
    public DateTimeOffset FechaRegistroUtc { get; set; } = DateTimeOffset.UtcNow;
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public long RowVersion { get; set; } = 1;
}
