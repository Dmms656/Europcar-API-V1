namespace RedCar.Shared.Events;

public sealed record EventEnvelope<TPayload>(
    Guid EventId,
    string EventType,
    int SchemaVersion,
    DateTimeOffset OccurredAt,
    Guid CorrelationId,
    Guid? CausationId,
    string Producer,
    TPayload Payload)
{
    public static EventEnvelope<TPayload> Create(
        string eventType,
        Guid correlationId,
        string producer,
        TPayload payload,
        Guid? causationId = null,
        int schemaVersion = 1) =>
        new(
            EventId: Guid.CreateVersion7(),
            EventType: eventType,
            SchemaVersion: schemaVersion,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: correlationId,
            CausationId: causationId,
            Producer: producer,
            Payload: payload);
}
