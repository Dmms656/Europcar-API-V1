using Middleware.RedCar.Api.Models.Settings;

namespace Middleware.RedCar.Api.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "BookingClients";

    public static IServiceCollection AddRedCarCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(o =>
        {
            o.AddPolicy(PolicyName, policy =>
            {
                if (origins.Length == 0)
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    return;
                }

                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
