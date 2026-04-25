namespace Europcar.Rental.DataManagement.Models;

public class LocalizacionModel
{
    public int IdLocalizacion { get; set; }
    public Guid LocalizacionGuid { get; set; }
    public string CodigoLocalizacion { get; set; } = string.Empty;
    public string NombreLocalizacion { get; set; } = string.Empty;
    public string DireccionLocalizacion { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string CorreoContacto { get; set; } = string.Empty;
    public string HorarioAtencion { get; set; } = string.Empty;
    public string? NombreCiudad { get; set; }
    public string EstadoLocalizacion { get; set; } = "ACT";
}
