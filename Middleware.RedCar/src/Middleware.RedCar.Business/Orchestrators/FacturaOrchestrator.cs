using Middleware.RedCar.Business.DTOs.Reservas;
using Middleware.RedCar.Business.Exceptions;
using Middleware.RedCar.Business.Interfaces;
using Middleware.RedCar.Business.Mappers;
using Middleware.RedCar.DataManagement.Interfaces;

namespace Middleware.RedCar.Business.Orchestrators;

public sealed class FacturaOrchestrator : IFacturaOrchestrator
{
    private readonly IReservasDataService _reservas;

    public FacturaOrchestrator(IReservasDataService reservas)
    {
        _reservas = reservas;
    }

    public async Task<FacturaBookingResponse> GetFacturaAsync(string codigoReserva, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigoReserva))
            throw new ValidationException(new[] { new ValidationFailure("codigoReserva", "codigoReserva es obligatorio.") });

        var factura = await _reservas.GetFacturaAsync(codigoReserva, ct)
            ?? throw new NotFoundException($"Factura para reserva {codigoReserva} no encontrada.");

        return ReservasBusinessMapper.ToBooking(factura);
    }
}
