namespace Europcar.Rental.DataManagement.Models;

public class ClienteModel
{
    public int IdCliente { get; set; }
    public Guid ClienteGuid { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Nombre1 { get; set; } = string.Empty;
    public string? Nombre2 { get; set; }
    public string Apellido1 { get; set; } = string.Empty;
    public string? Apellido2 { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? DireccionPrincipal { get; set; }
    public string EstadoCliente { get; set; } = "ACT";
    public long RowVersion { get; set; }
}
