namespace Europcar.Rental.Business.DTOs.Response.Catalogos;

public class LocalizacionResponse
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
    public string EstadoLocalizacion { get; set; } = string.Empty;
}

public class CatalogoResponse
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = string.Empty;
}
