using Grpc.Core;
using RedCar.Shared.Protos.Seguridad;

namespace RedCar.Seguridad.Api.Grpc;

/// <summary>
/// Implementacion del servicio gRPC SeguridadGrpc.
/// Por ahora solo Ping; en la Fase 3 anadimos ValidarToken, ResolverPermisos, etc.
/// </summary>
public sealed class SeguridadGrpcService : SeguridadGrpc.SeguridadGrpcBase
{
    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        var response = new PingResponse
        {
            Service = "RedCar.Seguridad",
            Version = typeof(SeguridadGrpcService).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            Echo = request.Caller ?? string.Empty,
            ServerTimeUtc = DateTimeOffset.UtcNow.ToString("O")
        };
        return Task.FromResult(response);
    }
}
