namespace Europcar.Rental.Business.DTOs.Response.Reservas;

public class ReservaResponse
{
    public int IdReserva { get; set; }
    public Guid ReservaGuid { get; set; }
    public string CodigoReserva { get; set; } = string.Empty;
    public string CodigoConfirmacion { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaHoraRecogida { get; set; }
    public DateTimeOffset FechaHoraDevolucion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ValorImpuestos { get; set; }
    public decimal ValorExtras { get; set; }
    public decimal CargoOneWay { get; set; }
    public decimal Total { get; set; }
    public string? NombreCliente { get; set; }
    public string? PlacaVehiculo { get; set; }

    /// <summary>
    /// Extras asociados a esta reserva.
    /// </summary>
    public List<ReservaExtraItemResponse> Extras { get; set; } = new();
}

/// <summary>
/// Detalle de un extra asociado a una reserva.
/// </summary>
public class ReservaExtraItemResponse
{
    public int IdReservaExtra { get; set; }
    public int IdExtra { get; set; }
    public string CodigoExtra { get; set; } = string.Empty;
    public string NombreExtra { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
