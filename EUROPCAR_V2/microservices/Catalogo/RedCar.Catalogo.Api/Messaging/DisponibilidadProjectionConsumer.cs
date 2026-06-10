using MassTransit;
using RedCar.Shared.Events;
using RedCar.Shared.Events.Reservas;

namespace RedCar.Catalogo.Api.Messaging;

/// <summary>Proyección en memoria de disponibilidad invalidada (fase 1). Sustituir por tabla persistente en fase 3.</summary>
public sealed class DisponibilidadProjectionStore
{
    private readonly HashSet<string> _keys = new(StringComparer.Ordinal);

    public void Invalidate(int idVehiculo, int idLocalizacion, string razon) =>
        _keys.Add($"{idVehiculo}:{idLocalizacion}:{razon}");

    public bool WasInvalidated(int idVehiculo, int idLocalizacion) =>
        _keys.Contains($"{idVehiculo}:{idLocalizacion}:reserva_creada")
        || _keys.Contains($"{idVehiculo}:{idLocalizacion}:reserva_cancelada");
}

public sealed class DisponibilidadProjectionConsumer :
    IConsumer<EventEnvelope<ReservaCreadaPayload>>,
    IConsumer<EventEnvelope<ReservaCanceladaPayload>>,
    IConsumer<EventEnvelope<DisponibilidadInvalidadaPayload>>
{
    private readonly DisponibilidadProjectionStore _store;
    private readonly ILogger<DisponibilidadProjectionConsumer> _logger;

    public DisponibilidadProjectionConsumer(
        DisponibilidadProjectionStore store,
        ILogger<DisponibilidadProjectionConsumer> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<EventEnvelope<ReservaCreadaPayload>> context)
    {
        var p = context.Message.Payload;
        _store.Invalidate(p.IdVehiculo, p.IdLocalizacionRecogida, "reserva_creada");
        _logger.LogInformation("Proyección: reserva creada vehiculo={V}", p.IdVehiculo);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<EventEnvelope<ReservaCanceladaPayload>> context)
    {
        var p = context.Message.Payload;
        _store.Invalidate(p.IdVehiculo, 0, "reserva_cancelada");
        _logger.LogInformation("Proyección: reserva cancelada vehiculo={V}", p.IdVehiculo);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<EventEnvelope<DisponibilidadInvalidadaPayload>> context)
    {
        var p = context.Message.Payload;
        _store.Invalidate(p.IdVehiculo, p.IdLocalizacion, p.Razon);
        return Task.CompletedTask;
    }
}
