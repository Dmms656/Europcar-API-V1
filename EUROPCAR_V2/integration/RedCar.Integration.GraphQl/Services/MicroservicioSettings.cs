namespace RedCar.Integration.GraphQl.Services;

public sealed class MicroserviciosGatewaySettings
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
    public int TimeoutSeconds { get; set; } = 60;
}
