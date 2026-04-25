namespace Europcar.Rental.DataManagement.Models;

/// <summary>
/// Modelo de localización extendido con Id de ciudad para Booking.
/// </summary>
public class BookingLocalizacionModel
{
    public int IdLocalizacion { get; set; }
    public string CodigoLocalizacion { get; set; } = string.Empty;
    public string NombreLocalizacion { get; set; } = string.Empty;
    public string DireccionLocalizacion { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string CorreoContacto { get; set; } = string.Empty;
    public string HorarioAtencion { get; set; } = string.Empty;
    public string ZonaHoraria { get; set; } = string.Empty;
    public int IdCiudad { get; set; }
    public string NombreCiudad { get; set; } = string.Empty;
}
