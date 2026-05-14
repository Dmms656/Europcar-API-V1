using Microsoft.Extensions.Logging;
using Middleware.RedCar.DataAccess.Clients.Interfaces;

namespace Middleware.RedCar.DataAccess.Clients;

/// <summary>
/// Cuando Seguridad corre en el mismo proceso que el middleware, no hay HTTP hacia otro host.
/// <see cref="ISeguridadDataService"/> solo usa <see cref="PingAsync"/> hoy.
/// </summary>
public sealed class LocalSeguridadClient : ISeguridadClient
{
    private static readonly DateTimeOffset StartedAtUtc = DateTimeOffset.UtcNow;
    private readonly ILogger<LocalSeguridadClient> _logger;

    public LocalSeguridadClient(ILogger<LocalSeguridadClient> logger) => _logger = logger;

    public Task<bool> PingAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Ping Seguridad (embebido): OK");
        return Task.FromResult(true);
    }

    public Task<ServiceInfoDto?> GetInfoAsync(CancellationToken ct = default) =>
        Task.FromResult<ServiceInfoDto?>(new ServiceInfoDto(
            Service: "Middleware+Auth",
            Schema: "security (embebido)",
            Version: "1",
            Environment: "embedded",
            StartedAtUtc: StartedAtUtc,
            Status: "running"));
}
