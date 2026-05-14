using Grpc.Core;
using RedCar.Shared.Protos.Catalogo;

namespace RedCar.Catalogo.Api.Grpc;

public sealed class CatalogoGrpcService : CatalogoGrpc.CatalogoGrpcBase
{
    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        var response = new PingResponse
        {
            Service = "RedCar.Catalogo",
            Version = typeof(CatalogoGrpcService).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            Echo = request.Caller ?? string.Empty,
            ServerTimeUtc = DateTimeOffset.UtcNow.ToString("O")
        };
        return Task.FromResult(response);
    }
}
