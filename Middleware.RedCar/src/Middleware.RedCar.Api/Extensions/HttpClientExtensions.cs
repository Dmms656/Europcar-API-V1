using Microsoft.Extensions.Options;
using Middleware.RedCar.Api.Models.Settings;
using Middleware.RedCar.DataAccess.Clients;
using Middleware.RedCar.DataAccess.Clients.Interfaces;

namespace Middleware.RedCar.Api.Extensions;

public static class HttpClientExtensions
{
    /// <summary>
    /// Registra los 5 HttpClients tipados hacia los microservicios + el handler
    /// que reenvia el JWT del usuario llamante.
    /// </summary>
    public static IServiceCollection AddMicroservicioHttpClients(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<BearerTokenPropagationHandler>();

        services.AddHttpClient<ISeguridadClient, SeguridadClient>(ConfigureClient<MicroserviciosSettings, SeguridadClient>(s => s.Seguridad))
            .AddHttpMessageHandler<BearerTokenPropagationHandler>();

        services.AddHttpClient<ICatalogoClient, CatalogoClient>(ConfigureClient<MicroserviciosSettings, CatalogoClient>(s => s.Catalogo))
            .AddHttpMessageHandler<BearerTokenPropagationHandler>();

        services.AddHttpClient<ILocalizacionesClient, LocalizacionesClient>(ConfigureClient<MicroserviciosSettings, LocalizacionesClient>(s => s.Localizaciones))
            .AddHttpMessageHandler<BearerTokenPropagationHandler>();

        services.AddHttpClient<IClientesClient, ClientesClient>(ConfigureClient<MicroserviciosSettings, ClientesClient>(s => s.Clientes))
            .AddHttpMessageHandler<BearerTokenPropagationHandler>();

        services.AddHttpClient<IReservasClient, ReservasClient>(ConfigureClient<MicroserviciosSettings, ReservasClient>(s => s.Reservas))
            .AddHttpMessageHandler<BearerTokenPropagationHandler>();

        // Llamadas a MS.Seguridad sin propagar el Bearer del cliente (login/registro desde SPA).
        services.AddHttpClient("SeguridadNoBearer", (sp, http) =>
        {
            var settings = sp.GetRequiredService<IOptions<MicroserviciosSettings>>().Value;
            var endpoint = settings.Seguridad;
            if (string.IsNullOrWhiteSpace(endpoint.BaseUrl))
            {
                throw new InvalidOperationException(
                    "Microservicios:Seguridad:BaseUrl no configurada. Revisa appsettings + .env.");
            }
            var baseUrl = endpoint.BaseUrl.TrimEnd('/') + "/";
            http.BaseAddress = new Uri(baseUrl);
            http.Timeout = TimeSpan.FromSeconds(endpoint.TimeoutSeconds <= 0 ? 10 : endpoint.TimeoutSeconds);
        });

        return services;
    }

    private static Action<IServiceProvider, HttpClient> ConfigureClient<TSettings, TClient>(Func<TSettings, MicroservicioEndpoint> selector)
        where TSettings : class, new()
        => (sp, http) =>
        {
            var settings = sp.GetRequiredService<IOptions<TSettings>>().Value;
            var endpoint = selector(settings);
            if (string.IsNullOrWhiteSpace(endpoint.BaseUrl))
            {
                throw new InvalidOperationException(
                    $"BaseUrl no configurada para microservicio de {typeof(TClient).Name}. Revisa appsettings + .env.");
            }
            http.BaseAddress = new Uri(endpoint.BaseUrl);
            http.Timeout = TimeSpan.FromSeconds(endpoint.TimeoutSeconds <= 0 ? 10 : endpoint.TimeoutSeconds);
        };
}

/// <summary>
/// Lee el Authorization header del request entrante y lo propaga al downstream.
/// Asi los microservicios reciben el mismo Bearer Token que envio el cliente Booking.
/// </summary>
public sealed class BearerTokenPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    public BearerTokenPropagationHandler(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var http = _accessor.HttpContext;
        if (http is not null)
        {
            var auth = http.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(auth) && request.Headers.Authorization is null)
            {
                if (System.Net.Http.Headers.AuthenticationHeaderValue.TryParse(auth, out var parsed))
                {
                    request.Headers.Authorization = parsed;
                }
            }
        }
        return base.SendAsync(request, cancellationToken);
    }
}
