namespace RedCar.Localizaciones.DataAccess.Entities;

public sealed class Localizacion
{
    public int IdLocalizacion { get; set; }
    public Guid LocalizacionGuid { get; set; }
    public string CodigoLocalizacion { get; set; } = string.Empty;
    public string NombreLocalizacion { get; set; } = string.Empty;
    public int IdCiudad { get; set; }
    public Ciudad? Ciudad { get; set; }
    public string DireccionLocalizacion { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string CorreoContacto { get; set; } = string.Empty;
    public string HorarioAtencion { get; set; } = string.Empty;
    public string ZonaHoraria { get; set; } = "America/Guayaquil";
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public string EstadoLocalizacion { get; set; } = "ACT";
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
