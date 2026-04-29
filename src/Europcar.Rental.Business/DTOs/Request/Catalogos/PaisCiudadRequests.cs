namespace Europcar.Rental.Business.DTOs.Request.Catalogos;

public class CrearPaisRequest
{
    public string CodigoIso2 { get; set; } = string.Empty;
    public string NombrePais { get; set; } = string.Empty;
}

public class ActualizarPaisRequest
{
    public string NombrePais { get; set; } = string.Empty;
}

public class CambiarEstadoPaisRequest
{
    public string Estado { get; set; } = "ACT";
    public string? Motivo { get; set; }
}

public class CrearCiudadRequest
{
    public int IdPais { get; set; }
    public string NombreCiudad { get; set; } = string.Empty;
}

public class ActualizarCiudadRequest
{
    public int IdPais { get; set; }
    public string NombreCiudad { get; set; } = string.Empty;
}

public class CambiarEstadoCiudadRequest
{
    public string Estado { get; set; } = "ACT";
    public string? Motivo { get; set; }
}
