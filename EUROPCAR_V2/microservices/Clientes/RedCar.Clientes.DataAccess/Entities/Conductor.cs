namespace RedCar.Clientes.DataAccess.Entities;

public sealed class Conductor
{
    public int IdConductor { get; set; }
    public Guid ConductorGuid { get; set; }
    public string CodigoConductor { get; set; } = string.Empty;
    public int? IdCliente { get; set; }
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string ConNombre1 { get; set; } = string.Empty;
    public string? ConNombre2 { get; set; }
    public string ConApellido1 { get; set; } = string.Empty;
    public string? ConApellido2 { get; set; }
    public string NumeroLicencia { get; set; } = string.Empty;
    public DateOnly FechaVencimientoLicencia { get; set; }
    public short EdadConductor { get; set; }
    public string ConTelefono { get; set; } = string.Empty;
    public string ConCorreo { get; set; } = string.Empty;
    public bool EsConductorJoven { get; set; }
    public string EstadoConductor { get; set; } = "ACT";
    public bool EsEliminado { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificadoDesdeIp { get; set; }
    public DateTimeOffset? FechaInhabilitacionUtc { get; set; }
    public string? MotivoInhabilitacion { get; set; }
    public string OrigenRegistro { get; set; } = string.Empty;
    public long RowVersion { get; set; }
}
