namespace Europcar.Rental.DataManagement.Models;

public class ConductorModel
{
    public int IdConductor { get; set; }
    public Guid ConductorGuid { get; set; }
    public string CodigoConductor { get; set; } = string.Empty;
    public int? IdCliente { get; set; }
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Nombre1 { get; set; } = string.Empty;
    public string? Nombre2 { get; set; }
    public string Apellido1 { get; set; } = string.Empty;
    public string? Apellido2 { get; set; }
    public string NumeroLicencia { get; set; } = string.Empty;
    public DateOnly FechaVencimientoLicencia { get; set; }
    public short EdadConductor { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public bool? EsConductorJoven { get; set; }
    public string EstadoConductor { get; set; } = "ACT";
    
    public string NombreCompleto => $"{Nombre1} {Apellido1}".Trim();
}
