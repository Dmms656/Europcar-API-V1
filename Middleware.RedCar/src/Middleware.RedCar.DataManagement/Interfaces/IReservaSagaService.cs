using Middleware.RedCar.DataAccess.GrpcClients.Interfaces;
using Middleware.RedCar.DataManagement.Models.Reservas;

namespace Middleware.RedCar.DataManagement.Interfaces;

public interface IReservaSagaService
{
    Task<CrearReservaGrpcResult> CrearReservaViaEventBusAsync(
        CrearReservaGrpcRequest request,
        CancellationToken ct = default);
}
