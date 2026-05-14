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
            options.Address = new Uri(settings.Reservas.BaseUrl);
        });

        services.AddScoped<IReservasGrpcClient, ReservasGrpcClient>();
        return services;
    }
}
