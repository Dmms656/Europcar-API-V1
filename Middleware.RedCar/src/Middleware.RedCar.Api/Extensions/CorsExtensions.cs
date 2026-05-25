using Middleware.RedCar.Api.Models.Settings;

namespace Middleware.RedCar.Api.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "BookingClients";

    public static IServiceCollection AddRedCarCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));

        var origins = (configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
            .Select(NormalizeOrigin)
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

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
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    /// <summary>El navegador envía Origin sin barra final; Render a veces la configura con /.</summary>
    private static string NormalizeOrigin(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin)) return string.Empty;
        return origin.Trim().TrimEnd('/');
    }
}
