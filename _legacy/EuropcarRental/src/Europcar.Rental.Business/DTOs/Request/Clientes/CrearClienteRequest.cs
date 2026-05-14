namespace Europcar.Rental.Business.DTOs.Request.Clientes;

public class CrearClienteRequest
{
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
}
