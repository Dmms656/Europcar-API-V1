using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataAccess.GrpcClients.Interfaces;
using Middleware.RedCar.DataManagement.Interfaces;
using Middleware.RedCar.DataManagement.Mappers;
using Middleware.RedCar.DataManagement.Models.Reservas;

namespace Middleware.RedCar.DataManagement.Services;

public sealed class ReservasDataService : IReservasDataService
{
    private readonly IReservasClient _restClient;
    private readonly IReservasGrpcClient _grpcClient;

    public ReservasDataService(IReservasClient restClient, IReservasGrpcClient grpcClient)
    {
        _restClient = restClient;
        _grpcClient = grpcClient;
    }

    public async Task<bool> VerificarDisponibilidadAsync(int idVehiculo, int idLocalizacion, DateTimeOffset fechaRecogida, DateTimeOffset fechaDevolucion, CancellationToken ct = default)
    {
        var dto = await _restClient.VerificarDisponibilidadAsync(idVehiculo, idLocalizacion, fechaRecogida, fechaDevolucion, ct);
        return dto?.Disponible ?? false;
    }

    public async Task<ReservaDataModel?> GetReservaAsync(string codigoReserva, CancellationToken ct = default)
    {
        var dto = await _restClient.GetReservaAsync(codigoReserva, ct);
        return dto is null ? null : ReservasDataMapper.ToData(dto);
    }

    public async Task<FacturaDataModel?> GetFacturaAsync(string codigoReserva, CancellationToken ct = default)
    {
        var dto = await _restClient.GetFacturaAsync(codigoReserva, ct);
        return dto is null ? null : ReservasDataMapper.ToData(dto);
    }

    public Task<CrearReservaGrpcResult> CrearReservaAsync(CrearReservaGrpcRequest request, CancellationToken ct = default)
        => _grpcClient.CrearReservaAsync(request, ct);

    public Task<CancelarReservaGrpcResult> CancelarReservaAsync(string codigoReserva, string motivo, string usuario, CancellationToken ct = default)
        => _grpcClient.CancelarReservaAsync(codigoReserva, motivo, usuario, ct);
}
