using MassTransit;
using Microsoft.EntityFrameworkCore;
using RedCar.Reservas.Api.Services;
using RedCar.Reservas.DataAccess.Context;
using RedCar.Shared.Events;
using RedCar.Shared.Events.Reservas;

namespace RedCar.Reservas.Api.Messaging;

/// <summary>Inicia saga: valida disponibilidad y delega upsert de cliente vía comando.</summary>
public sealed class ProcesarReservaBookingConsumer : IConsumer<ProcesarReservaBookingCommand>
{
    private readonly ReservasReadService _read;
    private readonly IPublishEndpoint _publish;
    private readonly ILogger<ProcesarReservaBookingConsumer> _logger;

    public ProcesarReservaBookingConsumer(
        ReservasReadService read,
        IPublishEndpoint publish,
        ILogger<ProcesarReservaBookingConsumer> logger)
    {
        _read = read;
        _publish = publish;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcesarReservaBookingCommand> context)
    {
        var cmd = context.Message;
        var ct = context.CancellationToken;

        var fechaRecogida = new DateTimeOffset(
            DateOnly.Parse(cmd.FechaInicio).ToDateTime(TimeOnly.Parse(cmd.HoraInicio), DateTimeKind.Utc));
        var fechaDevolucion = new DateTimeOffset(
            DateOnly.Parse(cmd.FechaFin).ToDateTime(TimeOnly.Parse(cmd.HoraFin), DateTimeKind.Utc));

        var disp = await _read.VerificarDisponibilidadAsync(
            cmd.IdVehiculo, cmd.IdLocalizacionRecogida, fechaRecogida, fechaDevolucion, ct);

        if (!disp.Disponible)
        {
            var rejected = EventEnvelope<ReservaRechazadaPayload>.Create(
                RoutingKeys.ReservaRechazada,
                cmd.CorrelationId,
                "RedCar.Reservas",
                new ReservaRechazadaPayload("El vehiculo no esta disponible para esas fechas.", cmd.IdVehiculo));
            await _publish.Publish(rejected, ct);
            return;
        }

        ReservasBookingSagaService.RegisterPending(cmd);

        _logger.LogInformation("Saga {CorrelationId}: delegando upsert cliente", cmd.CorrelationId);
        await _publish.Publish(new UpsertClienteCommand(cmd.CorrelationId, cmd.Cliente, cmd.Conductores), ct);
    }
}

/// <summary>Tras cliente actualizado, confirma la reserva y escribe outbox.</summary>
public sealed class ClienteActualizadoReservaConsumer : IConsumer<EventEnvelope<ClienteActualizadoPayload>>
{
    private readonly ReservasBookingSagaService _saga;
    private readonly ReservasDbContext _db;
    private readonly ILogger<ClienteActualizadoReservaConsumer> _logger;

    public ClienteActualizadoReservaConsumer(
        ReservasBookingSagaService saga,
        ReservasDbContext db,
        ILogger<ClienteActualizadoReservaConsumer> logger)
    {
        _saga = saga;
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventEnvelope<ClienteActualizadoPayload>> context)
    {
        var envelope = context.Message;
        var payload = envelope.Payload;
        var ct = context.CancellationToken;

        if (await _db.InboxProcessedEvents.AnyAsync(x => x.EventId == envelope.EventId, ct))
            return;

        var pending = ReservasBookingSagaService.TakePending(payload.CorrelationId);
        if (pending is null)
        {
            _logger.LogWarning("No pending booking command for correlation {Id}", payload.CorrelationId);
            return;
        }

        await _saga.CompletarReservaAsync(pending, payload, ct);

        _db.InboxProcessedEvents.Add(new DataAccess.Entities.InboxProcessedEvent
        {
            EventId = envelope.EventId,
            ProcessedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }
}
