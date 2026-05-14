using Microsoft.EntityFrameworkCore;
using RedCar.Seguridad.Business.Auth;
using RedCar.Seguridad.DataAccess.Context;

namespace Middleware.RedCar.Api.Extensions;

/// <summary>
/// Registra Auth + <see cref="SeguridadDbContext"/> en el mismo host que el middleware (un solo servicio en Render).
/// </summary>
public static class SeguridadEmbeddedExtensions
{
    public static IServiceCollection AddEmbeddedSeguridadAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Default")
            ?? configuration.GetConnectionString("Seguridad")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default__Seguridad")
            ?? string.Empty;

        services.AddDbContext<SeguridadDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseNpgsql(connectionString, npg => npg.EnableRetryOnFailure(3));
            }
            else
            {
                options.UseInMemoryDatabase("RedCar.Seguridad.Middleware.Dev");
            }
        });

        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
