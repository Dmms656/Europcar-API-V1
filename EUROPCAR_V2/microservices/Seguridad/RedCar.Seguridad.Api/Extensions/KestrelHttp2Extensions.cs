using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace RedCar.Seguridad.Api.Extensions;

/// <summary>
/// Habilita HTTP/2 (necesario para gRPC) en el listener por defecto.
/// REST sigue funcionando en HTTP/1.1 sobre el mismo puerto cuando se
/// usa Kestrel con AllowMultipleHttp2Connections + Http1AndHttp2.
/// </summary>
public static class KestrelHttp2Extensions
{
    public static WebApplicationBuilder ConfigureKestrelForRestAndGrpc(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureEndpointDefaults(o =>
            {
                o.Protocols = HttpProtocols.Http1AndHttp2;
            });
        });
        return builder;
    }
}
