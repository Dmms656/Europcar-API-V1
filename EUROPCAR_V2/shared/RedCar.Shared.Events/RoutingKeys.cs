namespace RedCar.Shared.Events;

/// <summary>Convención AMQP: redcar.&lt;dominio&gt;.&lt;entidad&gt;.&lt;verbo&gt;.v1</summary>
public static class RoutingKeys
{
    public const string ReservaCreada = "redcar.reservas.reserva.creada.v1";
    public const string ReservaCancelada = "redcar.reservas.reserva.cancelada.v1";
    public const string ReservaRechazada = "redcar.reservas.reserva.rechazada.v1";
    public const string ClienteActualizado = "redcar.clientes.cliente.actualizado.v1";
    public const string ConductoresRegistrados = "redcar.clientes.conductores.registrados.v1";
    public const string DisponibilidadInvalidada = "redcar.reservas.disponibilidad.invalidada.v1";
}

public static class EventBusEndpoints
{
    public const string EventsExchange = "redcar.events";
    public const string CommandsExchange = "redcar.commands";
    public const string ReservasCommandsQueue = "reservas.commands";
    public const string ClientesCommandsQueue = "clientes.commands";
    public const string MiddlewareSagaQueue = "middleware.saga.reservas";
    public const string CatalogoProjectionQueue = "catalogo.projection";
}
