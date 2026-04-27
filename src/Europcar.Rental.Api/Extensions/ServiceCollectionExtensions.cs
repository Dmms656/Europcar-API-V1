using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataManagement.Common;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Services;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.Business.Services;

namespace Europcar.Rental.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("RentalDb")
            + ";Timeout=10;Command Timeout=10;Maximum Pool Size=10;Connection Idle Lifetime=30;Connection Pruning Interval=5;";

        services.AddDbContext<RentalDbContext>(options =>
            options.UseNpgsql(connStr, npgsqlOptions =>
                npgsqlOptions.EnableRetryOnFailure(2)));

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // DataManagement
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IClienteDataService, ClienteDataService>();
        services.AddScoped<IVehiculoDataService, VehiculoDataService>();
        services.AddScoped<IReservaDataService, ReservaDataService>();
        services.AddScoped<IUsuarioDataService, UsuarioDataService>();
        services.AddScoped<IContratoDataService, ContratoDataService>();
        services.AddScoped<ICheckInOutDataService, CheckInOutDataService>();
        services.AddScoped<IPagoDataService, PagoDataService>();
        services.AddScoped<IMantenimientoDataService, MantenimientoDataService>();
        services.AddScoped<ILocalizacionDataService, LocalizacionDataService>();
        services.AddScoped<ICatalogoDataService, CatalogoDataService>();
        services.AddScoped<IExtraDataService, ExtraDataService>();
        services.AddScoped<IBookingDataService, BookingDataService>();
        services.AddScoped<IConductorDataService, ConductorDataService>();
        services.AddScoped<IFacturaDataService, FacturaDataService>();

        // Business
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IVehiculoService, VehiculoService>();
        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<IContratoService, ContratoService>();
        services.AddScoped<IPagoService, PagoService>();
        services.AddScoped<IMantenimientoService, MantenimientoService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JwtSettings");
        var secretKey = jwtSection["SecretKey"]!;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Europcar Rental API",
                Version = "v1",
                Description = "API REST para gestión de renta de vehículos - Europcar",
                Contact = new OpenApiContact
                {
                    Name = "Europcar Dev Team"
                }
            });

            // JWT en Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingrese el token JWT. Ejemplo: eyJhbGciOiJIUzI1NiIs..."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", builder =>
            {
                // Por motivos de seguridad, solo Swagger (mismo dominio) puede acceder.
                // Cuando el frontend esté listo, agrega su URL aquí en lugar de usar AllowAnyOrigin()
                builder.WithOrigins(
                        "http://localhost:5173",
                        "https://europcar-frontend.onrender.com",
                        "https://tu-frontend.vercel.app"
                       )
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        return services;
    }
}
