using MassTransit;
using RedCar.Shared.Events;
using RedCar.Shared.Events.Reservas;

namespace Middleware.RedCar.Api.Messaging;

public sealed class ReservaCreadaSagaConsumer : IConsumer<EventEnvelope<ReservaCreadaPayload>>
{
    private readonly ReservaSagaWaiter _waiter;

    public ReservaCreadaSagaConsumer(ReservaSagaWaiter waiter) => _waiter = waiter;

    public Task Consume(ConsumeContext<EventEnvelope<ReservaCreadaPayload>> context)
    {
        var envelope = context.Message;
        _waiter.Complete(envelope.CorrelationId, new ReservaSagaOutcome(true, envelope.Payload, null));
        return Task.CompletedTask;
    }
}

public sealed class ReservaRechazadaSagaConsumer : IConsumer<EventEnvelope<ReservaRechazadaPayload>>
{
    private readonly ReservaSagaWaiter _waiter;

    public ReservaRechazadaSagaConsumer(ReservaSagaWaiter waiter) => _waiter = waiter;

    public Task Consume(ConsumeContext<EventEnvelope<ReservaRechazadaPayload>> context)
    {
        var envelope = context.Message;
        _waiter.Complete(envelope.CorrelationId,
            new ReservaSagaOutcome(false, null, envelope.Payload.Motivo));
        return Task.CompletedTask;
    }
}
