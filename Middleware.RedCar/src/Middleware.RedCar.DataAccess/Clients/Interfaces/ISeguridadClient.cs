namespace Middleware.RedCar.DataAccess.Clients.Interfaces;

/// <summary>
/// Abstracción legacy para smoke tests; con auth embebido usa <see cref="LocalSeguridadClient"/>.
/// </summary>
public interface ISeguridadClient
{
    /// <summary>
    /// Comprueba que el MS responde (smoke test).
    /// </summary>
    Task<bool> PingAsync(CancellationToken ct = default);

    /// <summary>
    /// Recupera la informacion publica del servicio (GET /info).
    /// </summary>
    Task<ServiceInfoDto?> GetInfoAsync(CancellationToken ct = default);
}

public sealed record ServiceInfoDto(string Service, string Schema, string Version, string Environment, DateTimeOffset StartedAtUtc, string Status);
