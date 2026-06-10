using RedCar.Catalogo.Api.Messaging;
using RedCar.Shared.Messaging;

namespace RedCar.Catalogo.Api.Extensions;

public static class EventBusExtensions
{
    public static IServiceCollection AddCatalogoEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        if (!MassTransitExtensions.IsRabbitMqConfigured(configuration))
        {
            return services;
        }

        services.AddSingleton<DisponibilidadProjectionStore>();
        services.AddRedCarMassTransit(configuration, "catalogo", x =>
        {
            x.AddConsumer<DisponibilidadProjectionConsumer>();
        });
        return services;
    }
}
