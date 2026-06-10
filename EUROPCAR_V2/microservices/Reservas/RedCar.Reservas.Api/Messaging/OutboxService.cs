using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RedCar.Reservas.DataAccess.Context;
using RedCar.Reservas.DataAccess.Entities;
using RedCar.Shared.Events;

namespace RedCar.Reservas.Api.Messaging;

public sealed class OutboxService
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ReservasDbContext _db;

    public OutboxService(ReservasDbContext db) => _db = db;

    /// <summary>Encola en el mismo DbContext; persistir con el SaveChanges de la transacción de negocio.</summary>
    public void Stage<TPayload>(string eventType, Guid correlationId, TPayload payload)
    {
        var envelope = EventEnvelope<TPayload>.Create(eventType, correlationId, "RedCar.Reservas", payload);
        _db.OutboxMessages.Add(new OutboxMessage
        {
            EventId = envelope.EventId,
            EventType = envelope.EventType,
            CorrelationId = envelope.CorrelationId,
            PayloadJson = JsonSerializer.Serialize(envelope, Json),
            OccurredAt = envelope.OccurredAt
        });
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(int batchSize, CancellationToken ct) =>
        await _db.OutboxMessages
            .Where(x => x.PublishedAt == null)
            .OrderBy(x => x.OccurredAt)
            .Take(batchSize)
            .ToListAsync(ct);

    public async Task MarkPublishedAsync(long id, CancellationToken ct)
    {
        var row = await _db.OutboxMessages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (row is null) return;
        row.PublishedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
