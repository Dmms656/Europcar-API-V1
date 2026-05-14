namespace Europcar.Rental.DataManagement.Models;

public class ContratoModel
{
    public int IdContrato { get; set; }
    public Guid ContratoGuid { get; set; }
    public string NumeroContrato { get; set; } = string.Empty;
    public int IdReserva { get; set; }
    public int IdCliente { get; set; }
    public int IdVehiculo { get; set; }
    public DateTimeOffset FechaHoraSalida { get; set; }
    public DateTimeOffset FechaHoraPrevistaDevolucion { get; set; }
    public int KilometrajeSalida { get; set; }
    public decimal NivelCombustibleSalida { get; set; }
    public string EstadoContrato { get; set; } = "ABIERTO";
    public string? PdfUrl { get; set; }
    public string? ObservacionesContrato { get; set; }
    public string? NombreCliente { get; set; }
    public string? PlacaVehiculo { get; set; }
    public string? CodigoReserva { get; set; }
}
