namespace RedCar.Shared.Messaging;

public sealed class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/redcar-marketplace";
    public string Username { get; set; } = "redcar";
    public string Password { get; set; } = "redcar_dev";
}

public sealed class EventBusSettings
{
    public const string SectionName = "EvB";

    /// <summary>Usar saga RabbitMQ para crear/cancelar reserva (REST público sin cambios).</summary>
    public bool Enabled { get; set; }

    /// <summary>Timeout en segundos para esperar reserva.creada desde el middleware.</summary>
    public int SagaTimeoutSeconds { get; set; } = 90;
}

public sealed class IntegrationSettings
{
    public const string SectionName = "Integration";

    /// <summary>Leer microservicios vía gateway GraphQL en lugar de HTTP disperso.</summary>
    public bool UseGraphQl { get; set; }

    /// <summary>GraphQL en el mismo proceso que el middleware (un solo Web Service en Render).</summary>
    public bool EmbeddedGraphQl { get; set; }

    public string GraphQlBaseUrl { get; set; } = "http://localhost:5110/graphql";
}
