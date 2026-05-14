using FluentValidation;
using Middleware.RedCar.Api.Models.Settings;
using Middleware.RedCar.Business;
using Middleware.RedCar.Business.Interfaces;
using Middleware.RedCar.Business.Orchestrators;
using Middleware.RedCar.Business.Validators;
using Middleware.RedCar.DataManagement.Interfaces;
using Middleware.RedCar.DataManagement.Services;

namespace Middleware.RedCar.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra DataServices, Orchestrators y Validators del middleware.
    /// </summary>
    public static IServiceCollection AddRedCarMiddlewareServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Settings
        services.Configure<MicroserviciosSettings>(configuration.GetSection(MicroserviciosSettings.SectionName));
        services.Configure<NegocioSettings>(configuration.GetSection(NegocioSettings.SectionName));

        // Data services (DataManagement)
        services.AddScoped<ISeguridadDataService, SeguridadDataService>();
        services.AddScoped<ICatalogoDataService, CatalogoDataService>();
        services.AddScoped<ILocalizacionesDataService, LocalizacionesDataService>();
        services.AddScoped<IClientesDataService, ClientesDataService>();
        services.AddScoped<IReservasDataService, ReservasDataService>();

        // Orchestrators (Business)
        services.AddScoped<IMarketplaceOrchestrator, MarketplaceOrchestrator>();
        services.AddScoped<IReservaOrchestrator, ReservaOrchestrator>();
        services.AddScoped<IFacturaOrchestrator, FacturaOrchestrator>();

        // Validators (FluentValidation)
        services.AddValidatorsFromAssemblyContaining<CrearReservaValidator>();

        return services;
    }
}
