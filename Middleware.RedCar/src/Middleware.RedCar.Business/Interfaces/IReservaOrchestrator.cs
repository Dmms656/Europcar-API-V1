using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.Business.DTOs.Reservas;

namespace Middleware.RedCar.Business.Interfaces;

/// <summary>
/// Orquestador del flujo de reservas: disponibilidad, crear, consultar y cancelar.
/// Coordina MS.Catalogo, MS.Clientes y MS.Reservas (este ultimo via gRPC para crear).
/// </summary>
public interface IReservaOrchestrator
{
    Task<DisponibilidadResponse> VerificarDisponibilidadAsync(int idVehiculo, int idLocalizacion, DateTimeOffset fechaRecogida, DateTimeOffset fechaDevolucion, CancellationToken ct = default);
    Task<CrearReservaBookingResponse> CrearReservaAsync(CrearReservaBookingRequest request, CancellationToken ct = default);
    Task<ReservaBookingResponse> GetReservaAsync(string codigoReserva, CancellationToken ct = default);
    Task<CancelarReservaResponse> CancelarReservaAsync(string codigoReserva, CancelarReservaRequest request, string? usuarioCancelacion, CancellationToken ct = default);
}
