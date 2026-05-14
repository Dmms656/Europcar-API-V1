using Middleware.RedCar.Business.DTOs.Reservas;

namespace Middleware.RedCar.Business.Interfaces;

public interface IFacturaOrchestrator
{
    Task<FacturaBookingResponse> GetFacturaAsync(string codigoReserva, CancellationToken ct = default);
}
