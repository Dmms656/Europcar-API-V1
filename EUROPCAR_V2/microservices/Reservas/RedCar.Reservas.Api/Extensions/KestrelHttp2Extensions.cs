using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace RedCar.Reservas.Api.Extensions;

public static class KestrelHttp2Extensions
{
    public static WebApplicationBuilder ConfigureKestrelForRestAndGrpc(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http1AndHttp2);
        });
        return builder;
    }
}
