using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RedCar.Shared.Messaging;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRedCarMassTransit(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

        var rabbit = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        // CloudAMQP/LavinMQ usa vhost sin barra (ej. nendzadb); local Docker suele usar /redcar-marketplace.
        var virtualHost = rabbit.VirtualHost.Trim();
        if (virtualHost.Length > 1 && virtualHost.StartsWith('/'))
            virtualHost = virtualHost.TrimStart('/');

        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbit.Host, rabbit.Port, virtualHost, h =>
                {
                    h.Username(rabbit.Username);
                    h.Password(rabbit.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
