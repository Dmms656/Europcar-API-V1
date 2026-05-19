namespace RedCar.Clientes.DataAccess.Entities;

public sealed class Cliente
{
    public int IdCliente { get; set; }
    public Guid ClienteGuid { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string CliNombre1 { get; set; } = string.Empty;
    public string? CliNombre2 { get; set; }
    public string CliApellido1 { get; set; } = string.Empty;
    public string? CliApellido2 { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public string CliTelefono { get; set; } = string.Empty;
    public string CliCorreo { get; set; } = string.Empty;
    public string? DireccionPrincipal { get; set; }
    public string EstadoCliente { get; set; } = "ACT";
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
