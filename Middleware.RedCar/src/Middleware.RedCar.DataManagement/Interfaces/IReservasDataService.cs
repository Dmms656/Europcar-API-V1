using Middleware.RedCar.DataAccess.GrpcClients.Interfaces;
using Middleware.RedCar.DataManagement.Models.Reservas;

namespace Middleware.RedCar.DataManagement.Interfaces;

public interface IReservasDataService
{
    Task<bool> VerificarDisponibilidadAsync(int idVehiculo, int idLocalizacion, DateTimeOffset fechaRecogida, DateTimeOffset fechaDevolucion, CancellationToken ct = default);
    Task<ReservaDataModel?> GetReservaAsync(string codigoReserva, CancellationToken ct = default);
    Task<FacturaDataModel?> GetFacturaAsync(string codigoReserva, CancellationToken ct = default);

    /// <summary>
    /// Crea una reserva. Internamente usa gRPC por su naturaleza transaccional
    /// (debe escribir reservas + res_x_con + res_x_xtras en una unica transaccion).
    /// </summary>
    Task<CrearReservaGrpcResult> CrearReservaAsync(CrearReservaGrpcRequest request, CancellationToken ct = default);

    Task<CancelarReservaGrpcResult> CancelarReservaAsync(string codigoReserva, string motivo, string usuario, CancellationToken ct = default);
}
