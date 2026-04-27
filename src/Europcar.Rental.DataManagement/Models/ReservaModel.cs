namespace Europcar.Rental.DataManagement.Models;

public class ReservaModel
{
    public int IdReserva { get; set; }
    public Guid ReservaGuid { get; set; }
    public string CodigoReserva { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public int IdVehiculo { get; set; }
    public int IdLocalizacionRecogida { get; set; }
    public int IdLocalizacionDevolucion { get; set; }
    public string CanalReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaHoraRecogida { get; set; }
    public DateTimeOffset FechaHoraDevolucion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ValorImpuestos { get; set; }
    public decimal ValorExtras { get; set; }
    public decimal CargoOneWay { get; set; }
    public decimal Total { get; set; }
    public string CodigoConfirmacion { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = "PENDIENTE";
    public string? NombreCliente { get; set; }
    public string? PlacaVehiculo { get; set; }
    public string? DescripcionVehiculo { get; set; }
    public List<ReservaExtraModel> Extras { get; set; } = new();
}
