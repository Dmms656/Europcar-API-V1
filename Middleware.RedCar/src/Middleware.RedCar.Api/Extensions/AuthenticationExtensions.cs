using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RedCar.Shared.Auth;

namespace Middleware.RedCar.Api.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Registra Bearer JWT con la misma simetrica que usan los microservicios.
    /// </summary>
    public static IServiceCollection AddRedCarAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(section);
        var settings = section.Get<JwtSettings>() ?? new JwtSettings();

        if (string.IsNullOrWhiteSpace(settings.SecretKey))
        {
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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (!string.IsNullOrEmpty(context.Token))
                            return Task.CompletedTask;

                        if (context.Request.Cookies.TryGetValue(AuthCookieExtensions.CookieName, out var cookieToken))
                            context.Token = cookieToken;

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}
