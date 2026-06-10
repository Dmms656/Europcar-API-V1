using System.Text.Json;
using MassTransit;
using RedCar.Shared.Events;
using RedCar.Shared.Events.Reservas;

namespace RedCar.Reservas.Api.Messaging;

public sealed class OutboxDispatcherHostedService : BackgroundService
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcherHostedService> _logger;

    public OutboxDispatcherHostedService(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcherHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Outbox dispatcher iteration failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<OutboxService>();
        var publish = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var batch = await outbox.GetUnpublishedAsync(20, ct);
        foreach (var row in batch)
        {
            try
            {
                await PublishRowAsync(publish, row, ct);
                await outbox.MarkPublishedAsync(row.Id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox id={Id} type={Type}", row.Id, row.EventType);
            }
        }
    }

    private static async Task PublishRowAsync(IPublishEndpoint publish, DataAccess.Entities.OutboxMessage row, CancellationToken ct)
    {
        if (row.EventType == RoutingKeys.ReservaCreada)
        {
            var envelope = JsonSerializer.Deserialize<EventEnvelope<ReservaCreadaPayload>>(row.PayloadJson, Json)!;
            await publish.Publish(envelope, ct);
            return;
        }

        if (row.EventType == RoutingKeys.ReservaCancelada)
        {
            var envelope = JsonSerializer.Deserialize<EventEnvelope<ReservaCanceladaPayload>>(row.PayloadJson, Json)!;
            await publish.Publish(envelope, ct);
            return;
        }

        if (row.EventType == RoutingKeys.ReservaRechazada)
        {
            var envelope = JsonSerializer.Deserialize<EventEnvelope<ReservaRechazadaPayload>>(row.PayloadJson, Json)!;
            await publish.Publish(envelope, ct);
            return;
        }

        if (row.EventType == RoutingKeys.DisponibilidadInvalidada)
        {
            var envelope = JsonSerializer.Deserialize<EventEnvelope<DisponibilidadInvalidadaPayload>>(row.PayloadJson, Json)!;
            await publish.Publish(envelope, ct);
        }
    }
}
