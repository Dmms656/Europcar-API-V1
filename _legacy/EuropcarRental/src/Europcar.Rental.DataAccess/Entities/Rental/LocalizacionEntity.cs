using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class LocalizacionEntity : BaseEstadoEntity
{
    public int IdLocalizacion { get; set; }
    public Guid LocalizacionGuid { get; set; }
    public string CodigoLocalizacion { get; set; } = string.Empty;
    public string NombreLocalizacion { get; set; } = string.Empty;
    public int IdCiudad { get; set; }
    public string DireccionLocalizacion { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string CorreoContacto { get; set; } = string.Empty;
    public string HorarioAtencion { get; set; } = string.Empty;
    public string ZonaHoraria { get; set; } = "America/Guayaquil";
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public string EstadoLocalizacion { get; set; } = "ACT";

    // Navigation
    public CiudadEntity Ciudad { get; set; } = null!;
}
