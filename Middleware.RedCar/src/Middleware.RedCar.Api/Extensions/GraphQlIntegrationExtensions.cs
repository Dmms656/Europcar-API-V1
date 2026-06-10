using Middleware.RedCar.DataAccess.GraphQl;
using RedCar.Shared.Messaging;

namespace Middleware.RedCar.Api.Extensions;

public static class GraphQlIntegrationExtensions
{
    public static IServiceCollection AddGraphQlIntegrationClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IntegrationSettings>(configuration.GetSection(IntegrationSettings.SectionName));
        services.AddHttpClient<GraphQlIntegrationClient>();
        return services;
    }
}
