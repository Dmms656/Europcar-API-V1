using Middleware.RedCar.Api.Messaging;
using Middleware.RedCar.DataManagement.Interfaces;
using RedCar.Shared.Events.Reservas;
using RedCar.Shared.Messaging;

namespace Middleware.RedCar.Api.Extensions;

public static class EventBusExtensions
{
    public static IServiceCollection AddRedCarEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EventBusSettings>(configuration.GetSection(EventBusSettings.SectionName));
        services.Configure<IntegrationSettings>(configuration.GetSection(IntegrationSettings.SectionName));

        services.AddSingleton<ReservaSagaWaiter>();
        services.AddScoped<IReservaSagaService, ReservaSagaService>();

        services.AddRedCarMassTransit(configuration, "middleware-redcar", x =>
        {
            x.AddConsumer<ReservaCreadaSagaConsumer>();
            x.AddConsumer<ReservaRechazadaSagaConsumer>();
        });

        return services;
    }
}
