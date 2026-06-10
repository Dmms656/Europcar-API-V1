using RedCar.Integration.GraphQl.Graph;
using RedCar.Integration.GraphQl.Services;

namespace Middleware.RedCar.Api.Extensions;

/// <summary>Expone /graphql en el mismo host que REST (sin segundo contenedor en Render).</summary>
public static class EmbeddedGraphQlExtensions
{
    public static IServiceCollection AddEmbeddedGraphQlGateway(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MicroserviciosGatewaySettings>(
            configuration.GetSection(MicroserviciosGatewaySettings.SectionName));

        services.AddHttpContextAccessor();
        services.AddHttpClient(nameof(MsHttpGateway));
        services.AddScoped<MsHttpGateway>();

        services
            .AddGraphQLServer()
            .AddQueryType<BookingQuery>()
            .ModifyRequestOptions(o =>
                o.IncludeExceptionDetails = configuration.GetValue("ASPNETCORE_ENVIRONMENT", "Production")
                    .Equals("Development", StringComparison.OrdinalIgnoreCase));

        return services;
    }
}
