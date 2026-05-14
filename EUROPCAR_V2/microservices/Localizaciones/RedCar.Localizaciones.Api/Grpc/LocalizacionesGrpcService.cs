using Grpc.Core;
using RedCar.Shared.Protos.Localizaciones;

namespace RedCar.Localizaciones.Api.Grpc;

public sealed class LocalizacionesGrpcService : LocalizacionesGrpc.LocalizacionesGrpcBase
{
    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        var response = new PingResponse
        {
            Service = "RedCar.Localizaciones",
            Version = typeof(LocalizacionesGrpcService).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            Echo = request.Caller ?? string.Empty,
            ServerTimeUtc = DateTimeOffset.UtcNow.ToString("O")
        };
        return Task.FromResult(response);
    }
}
