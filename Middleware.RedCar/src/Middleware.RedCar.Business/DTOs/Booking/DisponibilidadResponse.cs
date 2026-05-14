namespace Middleware.RedCar.Business.DTOs.Booking;

/// <summary>
/// Response del Endpoint 3 (GET /api/v2/booking/reservas/{idVehiculo}/disponibilidad).
/// </summary>
public sealed class DisponibilidadResponse
{
    public int IdVehiculo { get; set; }
    public int IdLocalizacion { get; set; }
    public DisponibilidadDetalle Disponibilidad { get; set; } = new();
}

public sealed class DisponibilidadDetalle
{
    public DateTimeOffset FechaRecogida { get; set; }
    public DateTimeOffset FechaDevolucion { get; set; }
    public bool Disponible { get; set; }
}
