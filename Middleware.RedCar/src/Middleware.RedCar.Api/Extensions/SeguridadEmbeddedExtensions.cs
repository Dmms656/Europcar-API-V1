using Microsoft.EntityFrameworkCore;
using Npgsql;
using RedCar.Seguridad.Business.Auth;
using RedCar.Seguridad.DataAccess.Context;

namespace Middleware.RedCar.Api.Extensions;

/// <summary>
/// Registra Auth + <see cref="SeguridadDbContext"/> en el mismo host que el middleware (un solo servicio en Render).
/// </summary>
public static class SeguridadEmbeddedExtensions
{
    /// <summary>
    /// El pooler transaccional de Supabase (puerto 6543) no combina bien con prepared statements implícitos ni multiplexing;
    /// sin estos flags Npgsql suele lanzar "Exception while reading from stream".
    /// </summary>
    private static string ApplySupabasePoolerDefaults(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return connectionString;
        if (!connectionString.Contains("pooler.supabase.com", StringComparison.OrdinalIgnoreCase))
            return connectionString;

        var b = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Multiplexing = false,
            NoResetOnClose = true,
            MaxAutoPrepare = 0
        };
        return b.ConnectionString;
    }

    public static IServiceCollection AddEmbeddedSeguridadAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Default")
            ?? configuration.GetConnectionString("Seguridad")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default__Seguridad")
            ?? string.Empty;

        connectionString = ApplySupabasePoolerDefaults(connectionString);

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
