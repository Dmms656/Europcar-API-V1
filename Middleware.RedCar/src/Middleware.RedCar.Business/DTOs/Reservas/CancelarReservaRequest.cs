namespace Middleware.RedCar.Business.DTOs.Reservas;

/// <summary>
/// Body del Endpoint 10 (PATCH /api/v2/booking/reservas/{codigoReserva}/cancelar).
/// Inferido del contrato (los _links incluyen "cancelar" y la tabla 0 declara PATCH).
/// </summary>
public sealed class CancelarReservaRequest
{
    public string MotivoCancelacion { get; set; } = string.Empty;
}

public sealed class CancelarReservaResponse
{
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaCancelacionUtc { get; set; }
    public string MotivoCancelacion { get; set; } = string.Empty;
}
