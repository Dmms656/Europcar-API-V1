using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataAccess.GrpcClients.Interfaces;
using Middleware.RedCar.DataManagement.Interfaces;
using Middleware.RedCar.DataManagement.Mappers;
using Middleware.RedCar.DataManagement.Models.Reservas;

namespace Middleware.RedCar.DataManagement.Services;

public sealed class ReservasDataService : IReservasDataService
{
    private readonly IReservasClient _restClient;

    public ReservasDataService(IReservasClient restClient) => _restClient = restClient;

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

    public async Task<CrearReservaGrpcResult> CrearReservaAsync(CrearReservaGrpcRequest request, CancellationToken ct = default)
    {
        var writeReq = new CrearReservaWriteRequest(
            request.IdVehiculo,
            request.IdLocalizacionRecogida,
            request.IdLocalizacionDevolucion,
            request.FechaInicio,
            request.FechaFin,
            request.HoraInicio,
            request.HoraFin,
            request.Observaciones,
            request.OrigenCanalReserva,
            request.IdCliente,
            new CrearReservaWriteCliente(
                request.Cliente.Nombres, request.Cliente.Apellidos,
                request.Cliente.TipoIdentificacion, request.Cliente.NumeroIdentificacion,
                request.Cliente.Correo, request.Cliente.Telefono),
            request.Conductores.Select(c => new CrearReservaWriteConductor(
                c.IdConductor, c.Nombres, c.Apellidos, c.TipoIdentificacion, c.NumeroIdentificacion,
                c.FechaVencimientoLicencia, c.EdadConductor, c.Correo, c.Telefono, c.EsPrincipal)).ToList(),
            request.Extras.Select(e => new CrearReservaWriteExtra(e.IdExtra, e.Cantidad)).ToList());

        var r = await _restClient.CrearReservaAsync(writeReq, ct);
        return new CrearReservaGrpcResult(
            r.CodigoReserva, r.EstadoReserva, r.FechaReservaUtc, r.CantidadDias,
            r.SubtotalVehiculo, r.SubtotalExtras, r.Subtotal, r.Iva, r.Total);
    }

    public async Task<CancelarReservaGrpcResult> CancelarReservaAsync(string codigoReserva, string motivo, string usuario, CancellationToken ct = default)
    {
        var r = await _restClient.CancelarReservaAsync(codigoReserva, motivo, usuario, ct);
        return new CancelarReservaGrpcResult(r.CodigoReserva, r.EstadoReserva, r.FechaCancelacionUtc);
    }
}
