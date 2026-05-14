using Grpc.Core;
using RedCar.Shared.Protos.Reservas;

namespace RedCar.Reservas.Api.Grpc;

public sealed class ReservasGrpcService : ReservasGrpc.ReservasGrpcBase
{
    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        var response = new PingResponse
        {
            Service = "RedCar.Reservas",
            Version = typeof(ReservasGrpcService).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            Echo = request.Caller ?? string.Empty,
            ServerTimeUtc = DateTimeOffset.UtcNow.ToString("O")
        };
        return Task.FromResult(response);
    }
}
