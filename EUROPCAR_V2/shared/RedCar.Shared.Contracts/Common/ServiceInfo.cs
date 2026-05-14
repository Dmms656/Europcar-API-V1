namespace RedCar.Shared.Contracts.Common;

/// <summary>
/// Payload del endpoint GET /info de cada microservicio.
/// Lo lee el orquestador para verificar la identidad del servicio.
/// </summary>
public sealed record ServiceInfo
{
    public required string Service { get; init; }
    public required string Schema { get; init; }
    public required string Version { get; init; }
    public required string Environment { get; init; }
    public required DateTimeOffset StartedAtUtc { get; init; }
    public required string Status { get; init; }
}
