namespace RedCar.Localizaciones.Api.Contracts;

public sealed class CiudadDto
{
    public int IdCiudad { get; init; }
    public Guid CiudadGuid { get; init; }
    public int IdPais { get; init; }
    public string NombreCiudad { get; init; } = string.Empty;
    public string EstadoCiudad { get; init; } = string.Empty;
}

public sealed class PaisDto
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Estado { get; init; } = "ACT";
}

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

public sealed class ActualizarLocalizacionRequest : CrearLocalizacionRequest;

public sealed class CrearCiudadRequest
{
    public int IdPais { get; set; } = 1;
    public string NombreCiudad { get; set; } = string.Empty;
}

public sealed class CambiarEstadoRequest
{
    public string Estado { get; set; } = "ACT";
    public string? Motivo { get; set; }
}
