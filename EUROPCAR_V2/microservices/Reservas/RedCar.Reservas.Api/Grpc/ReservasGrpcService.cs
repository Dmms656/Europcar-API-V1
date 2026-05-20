using Grpc.Core;
using RedCar.Reservas.Api.Services;
using RedCar.Shared.Protos.Reservas;

namespace RedCar.Reservas.Api.Grpc;

public sealed class ReservasGrpcService : ReservasGrpc.ReservasGrpcBase
{
    private readonly ReservasWriteService _write;

    public ReservasGrpcService(ReservasWriteService write) => _write = write;

    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        var response = new PingResponse
        {
            Service = "RedCar.Reservas",
            Echo = request.Caller ?? string.Empty,
            ServerTimeUtc = DateTimeOffset.UtcNow.ToString("O")
        };
        return Task.FromResult(response);
    }

    public override async Task<CrearReservaResponse> CrearReserva(CrearReservaRequest request, ServerCallContext context)
    {
        try
        {
            return await _write.CrearReservaAsync(request, context.CancellationToken);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.InnerException?.Message ?? ex.Message));
        }
    }

    public override async Task<CancelarReservaResponse> CancelarReserva(CancelarReservaRequest request, ServerCallContext context)
    {
        try
        {
            return await _write.CancelarReservaAsync(request, context.CancellationToken);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.InnerException?.Message ?? ex.Message));
        }
    }
}
