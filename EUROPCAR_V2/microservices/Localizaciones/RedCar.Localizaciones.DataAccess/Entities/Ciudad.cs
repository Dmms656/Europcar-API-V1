namespace RedCar.Localizaciones.DataAccess.Entities;

public sealed class Ciudad
{
    public int IdCiudad { get; set; }
    public Guid CiudadGuid { get; set; }
    public int IdPais { get; set; }
    public string NombreCiudad { get; set; } = string.Empty;
    public string EstadoCiudad { get; set; } = "ACT";
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
