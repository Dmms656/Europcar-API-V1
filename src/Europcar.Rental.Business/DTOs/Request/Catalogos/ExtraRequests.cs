namespace Europcar.Rental.Business.DTOs.Request.Catalogos;

public class CrearExtraRequest
{
    public string CodigoExtra { get; set; } = string.Empty;
    public string NombreExtra { get; set; } = string.Empty;
    public string? DescripcionExtra { get; set; }
    public string TipoExtra { get; set; } = "SERVICIO";
    public bool RequiereStock { get; set; } = false;
    public decimal ValorFijo { get; set; }
}

public class ActualizarExtraRequest
{
    public string NombreExtra { get; set; } = string.Empty;
    public string? DescripcionExtra { get; set; }
    public string TipoExtra { get; set; } = "SERVICIO";
    public bool RequiereStock { get; set; } = false;
    public decimal ValorFijo { get; set; }
}

public class CambiarEstadoExtraRequest
{
    public string Estado { get; set; } = "ACT";
    public string? Motivo { get; set; }
}
