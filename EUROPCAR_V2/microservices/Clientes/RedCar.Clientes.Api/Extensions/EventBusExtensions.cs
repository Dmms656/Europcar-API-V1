using RedCar.Clientes.Api.Messaging;
using RedCar.Clientes.Api.Services;
using RedCar.Shared.Messaging;

namespace RedCar.Clientes.Api.Extensions;

public static class EventBusExtensions
{
    public static IServiceCollection AddClientesEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        if (!MassTransitExtensions.IsRabbitMqConfigured(configuration))
        {
            return services;
        }

        services.AddScoped<ClientesWriteService>();
        services.AddRedCarMassTransit(configuration, "clientes", x =>
        {
            x.AddConsumer<UpsertClienteConsumer>();
        });
        return services;
    }
}
