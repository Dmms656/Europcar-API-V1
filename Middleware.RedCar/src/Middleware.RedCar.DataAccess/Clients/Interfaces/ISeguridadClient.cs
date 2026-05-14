namespace Middleware.RedCar.DataAccess.Clients.Interfaces;

/// <summary>
/// Cliente REST hacia MS.Seguridad. El middleware lo usa para validar tokens
/// o resolver datos del usuario autenticado cuando los necesita reenviar a otros MS.
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
