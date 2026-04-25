namespace Europcar.Rental.DataAccess.Entities.Security;

public class SesionEntity
{
    public long IdSesion { get; set; }
    public Guid SesionGuid { get; set; }
    public int IdUsuario { get; set; }
    public string TokenId { get; set; } = string.Empty;
    public string? RefreshTokenHash { get; set; }
    public string? IpOrigen { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset FechaInicioUtc { get; set; }
    public DateTimeOffset FechaExpiracionUtc { get; set; }
    public DateTimeOffset? FechaCierreUtc { get; set; }
    public string EstadoSesion { get; set; } = "ACTIVA";
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public long RowVersion { get; set; } = 1;

    // Navigation
    public UsuarioAppEntity Usuario { get; set; } = null!;
}
