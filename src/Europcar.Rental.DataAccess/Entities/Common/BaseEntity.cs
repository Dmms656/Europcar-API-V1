namespace Europcar.Rental.DataAccess.Entities.Common;

/// <summary>
/// Clase base abstracta con campos de auditoría comunes a todas las entidades.
/// </summary>
public abstract class BaseEntity
{
    public bool EsEliminado { get; set; } = false;
    public DateTimeOffset FechaRegistroUtc { get; set; } = DateTimeOffset.UtcNow;
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificadoDesdeIp { get; set; }
    public long RowVersion { get; set; } = 1;
}

/// <summary>
/// Entidad base con campos de estado e inhabilitación.
/// </summary>
public abstract class BaseEstadoEntity : BaseEntity
{
    public string OrigenRegistro { get; set; } = string.Empty;
    public DateTimeOffset? FechaInhabilitacionUtc { get; set; }
    public string? MotivoInhabilitacion { get; set; }
}
