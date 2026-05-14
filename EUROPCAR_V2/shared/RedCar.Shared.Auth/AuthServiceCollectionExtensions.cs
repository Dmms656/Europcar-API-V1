using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace RedCar.Shared.Auth;

public static class AuthServiceCollectionExtensions
{
    /// <summary>
    /// Registra JWT Bearer con la misma configuracion en todos los microservicios.
    /// El secret debe ser el mismo en MS.Seguridad (que emite) y en los demas MS (que validan).
    /// </summary>
    public static IServiceCollection AddRedCarJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(section);

        var settings = section.Get<JwtSettings>() ?? new JwtSettings();
        if (string.IsNullOrWhiteSpace(settings.SecretKey))
        {
            // No truncamos el proceso: dejamos que falle al validar el primer token.
            // Esto facilita levantar el MS sin JWT configurado durante desarrollo temprano.
            settings.SecretKey = "DEV_ONLY_REPLACE_ME_DEV_ONLY_REPLACE_ME_32B";
        }

        var keyBytes = Encoding.UTF8.GetBytes(settings.SecretKey);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();
        return services;
    }
}
