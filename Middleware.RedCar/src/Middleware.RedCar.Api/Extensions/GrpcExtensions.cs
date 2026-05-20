using Microsoft.Extensions.Options;
using Middleware.RedCar.Api.Models.Settings;
using Middleware.RedCar.DataAccess.GrpcClients;
using Middleware.RedCar.DataAccess.GrpcClients.Interfaces;
using Middleware.RedCar.DataAccess.Protos.Reservas;

namespace Middleware.RedCar.Api.Extensions;

public static class GrpcExtensions
{
    /// <summary>
    /// Registra el gRPC client hacia MS.Reservas (operacion transaccional de crear reserva).
    /// </summary>
    public static IServiceCollection AddRedCarGrpcClients(this IServiceCollection services)
    {
        services.AddGrpcClient<ReservasGrpc.ReservasGrpcClient>((sp, options) =>
        {
            var settings = sp.GetRequiredService<IOptions<MicroserviciosSettings>>().Value;
            if (string.IsNullOrWhiteSpace(settings.Reservas.BaseUrl))
            {
                throw new InvalidOperationException("BaseUrl de Reservas no configurada para gRPC.");
            }
            var baseUrl = settings.Reservas.BaseUrl.Trim().TrimEnd('/');
            options.Address = new Uri(baseUrl + "/", UriKind.Absolute);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true
        });

        services.AddScoped<IReservasGrpcClient, ReservasGrpcClient>();
        return services;
    }
}
