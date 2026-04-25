using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class ConductorEntity : BaseEstadoEntity
{
    public int IdConductor { get; set; }
    public Guid ConductorGuid { get; set; }
    public string CodigoConductor { get; set; } = string.Empty;
    public int? IdCliente { get; set; }
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string ConNombre1 { get; set; } = string.Empty;
    public string? ConNombre2 { get; set; }
    public string ConApellido1 { get; set; } = string.Empty;
    public string? ConApellido2 { get; set; }
    public string NumeroLicencia { get; set; } = string.Empty;
    public DateOnly FechaVencimientoLicencia { get; set; }
    public short EdadConductor { get; set; }
    public string ConTelefono { get; set; } = string.Empty;
    public string ConCorreo { get; set; } = string.Empty;
    public bool? EsConductorJoven { get; set; } // GENERATED column
    public string EstadoConductor { get; set; } = "ACT";

    // Navigation
    public ClienteEntity? Cliente { get; set; }
}
