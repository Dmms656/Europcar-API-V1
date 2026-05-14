using Grpc.Core;
using RedCar.Shared.Protos.Clientes;

namespace RedCar.Clientes.Api.Grpc;

public sealed class ClientesGrpcService : ClientesGrpc.ClientesGrpcBase
{
    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        var response = new PingResponse
        {
            Service = "RedCar.Clientes",
            Version = typeof(ClientesGrpcService).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            Echo = request.Caller ?? string.Empty,
            ServerTimeUtc = DateTimeOffset.UtcNow.ToString("O")
        };
        return Task.FromResult(response);
    }
}
