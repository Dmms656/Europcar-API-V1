using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class ClienteEntity : BaseEstadoEntity
{
    public int IdCliente { get; set; }
    public Guid ClienteGuid { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string CliNombre1 { get; set; } = string.Empty;
    public string? CliNombre2 { get; set; }
    public string CliApellido1 { get; set; } = string.Empty;
    public string? CliApellido2 { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public string CliTelefono { get; set; } = string.Empty;
    public string CliCorreo { get; set; } = string.Empty;
    public string? DireccionPrincipal { get; set; }
    public string EstadoCliente { get; set; } = "ACT";

    // Navigation
    public ICollection<ConductorEntity> Conductores { get; set; } = new List<ConductorEntity>();
    public ICollection<ReservaEntity> Reservas { get; set; } = new List<ReservaEntity>();
}
