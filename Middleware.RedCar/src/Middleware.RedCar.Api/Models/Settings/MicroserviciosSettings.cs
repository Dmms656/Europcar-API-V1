namespace Middleware.RedCar.Api.Models.Settings;

/// <summary>
/// Settings de las URLs y timeouts hacia los 5 microservicios.
/// </summary>
public sealed class MicroserviciosSettings
{
    public const string SectionName = "Microservicios";

    public MicroservicioEndpoint Catalogo { get; set; } = new();
    public MicroservicioEndpoint Localizaciones { get; set; } = new();
    public MicroservicioEndpoint Clientes { get; set; } = new();
    public MicroservicioEndpoint Reservas { get; set; } = new();
}

public sealed class MicroservicioEndpoint
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
}
