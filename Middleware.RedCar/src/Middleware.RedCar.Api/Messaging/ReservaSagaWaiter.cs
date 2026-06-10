using System.Collections.Concurrent;
using RedCar.Shared.Events.Reservas;

namespace Middleware.RedCar.Api.Messaging;

public sealed class ReservaSagaWaiter
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ReservaSagaOutcome>> _pending = new();

    public Task<ReservaSagaOutcome> WaitAsync(Guid correlationId, TimeSpan timeout, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<ReservaSagaOutcome>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pending.TryAdd(correlationId, tcs))
            throw new InvalidOperationException($"Saga {correlationId} ya en curso.");

        ct.Register(() => tcs.TrySetCanceled(ct));
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(timeout, ct);
                if (_pending.TryRemove(correlationId, out var pending))
                    pending.TrySetException(new TimeoutException($"Saga reserva {correlationId} expiró."));
            }
            catch (OperationCanceledException) { }
        }, ct);

        return tcs.Task;
    }

    public void Complete(Guid correlationId, ReservaSagaOutcome outcome)
    {
        if (_pending.TryRemove(correlationId, out var tcs))
            tcs.TrySetResult(outcome);
    }
}

public sealed record ReservaSagaOutcome(
    bool Success,
    ReservaCreadaPayload? Creada,
    string? MotivoRechazo);
