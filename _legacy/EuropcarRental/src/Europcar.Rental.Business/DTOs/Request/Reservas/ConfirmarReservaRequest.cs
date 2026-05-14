namespace Europcar.Rental.Business.DTOs.Request.Reservas;

public class ConfirmarReservaRequest
{
    public decimal? Monto { get; set; }
    public string? ReferenciaExterna { get; set; }
}
