namespace RedCar.Localizaciones.DataAccess.Entities;

public sealed class Localizacion
{
    public int IdLocalizacion { get; set; }
    public string CodigoLocalizacion { get; set; } = string.Empty;
    public string NombreLocalizacion { get; set; } = string.Empty;
    public int IdCiudad { get; set; }
    public Ciudad? Ciudad { get; set; }
    public string DireccionLocalizacion { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string CorreoContacto { get; set; } = string.Empty;
    public string HorarioAtencion { get; set; } = string.Empty;
    public string ZonaHoraria { get; set; } = "America/Guayaquil";
    public string EstadoLocalizacion { get; set; } = "ACT";
    public bool EsEliminado { get; set; }
}
