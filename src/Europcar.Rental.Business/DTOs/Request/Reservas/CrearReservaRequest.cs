namespace Europcar.Rental.Business.DTOs.Request.Reservas;

public class CrearReservaRequest
{
    public int IdCliente { get; set; }
    public int IdVehiculo { get; set; }
    public int IdLocalizacionRecogida { get; set; }
    public int IdLocalizacionDevolucion { get; set; }
    public string CanalReserva { get; set; } = "API";
    public DateTimeOffset FechaHoraRecogida { get; set; }
    public DateTimeOffset FechaHoraDevolucion { get; set; }

    /// <summary>
    /// Lista opcional de extras a contratar con la reserva.
    /// </summary>
    public List<ReservaExtraItemRequest> Extras { get; set; } = new();

    /// <summary>
    /// Lista de conductores asociados a la reserva. 
    /// Si no se envía, se asigna automáticamente el cliente como conductor principal.
    /// </summary>
    public List<ReservaConductorItemRequest> Conductores { get; set; } = new();
}

/// <summary>
/// Item individual de extra a asociar a una reserva.
/// </summary>
public class ReservaExtraItemRequest
{
    public int IdExtra { get; set; }
    public int Cantidad { get; set; } = 1;
}

/// <summary>
/// Item de conductor a asociar a una reserva.
/// </summary>
public class ReservaConductorItemRequest
{
    public int IdConductor { get; set; }
    public bool EsPrincipal { get; set; } = false;
}
