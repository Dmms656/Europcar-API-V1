namespace Europcar.Rental.Business.DTOs.Request.Localizaciones;

/// <summary>
/// Request para crear una nueva localización (sucursal).
/// </summary>
public class CrearLocalizacionRequest
{
    public string CodigoLocalizacion { get; set; } = string.Empty;
    public string NombreLocalizacion { get; set; } = string.Empty;
    public int IdCiudad { get; set; }
    public string DireccionLocalizacion { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string CorreoContacto { get; set; } = string.Empty;
    public string HorarioAtencion { get; set; } = string.Empty;
    public string? ZonaHoraria { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}

/// <summary>
/// Request para actualizar una localización existente.
/// El código es inmutable y no se incluye.
/// </summary>
public class ActualizarLocalizacionRequest
{
    public string NombreLocalizacion { get; set; } = string.Empty;
    public int IdCiudad { get; set; }
    public string DireccionLocalizacion { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string CorreoContacto { get; set; } = string.Empty;
    public string HorarioAtencion { get; set; } = string.Empty;
    public string? ZonaHoraria { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}

/// <summary>
/// Request para activar/inhabilitar una localización.
/// </summary>
public class CambiarEstadoLocalizacionRequest
{
    public string Estado { get; set; } = "ACT";
    public string? Motivo { get; set; }
}
