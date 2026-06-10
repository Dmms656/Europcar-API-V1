using MassTransit;
using RedCar.Reservas.Api.Messaging;
using RedCar.Shared.Events.Reservas;
using RedCar.Shared.Messaging;

namespace RedCar.Reservas.Api.Extensions;

public static class EventBusExtensions
{
    public static IServiceCollection AddReservasEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<OutboxService>();
        services.AddScoped<ReservasBookingSagaService>();
        services.AddHostedService<OutboxDispatcherHostedService>();

        services.AddRedCarMassTransit(configuration, "reservas", x =>
        {
            x.AddConsumer<ProcesarReservaBookingConsumer>();
            x.AddConsumer<ClienteActualizadoReservaConsumer>();
        });

        return services;
    }
}
