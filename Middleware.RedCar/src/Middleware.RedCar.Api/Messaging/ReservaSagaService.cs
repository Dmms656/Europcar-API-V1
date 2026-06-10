using MassTransit;
using Microsoft.Extensions.Options;
using Middleware.RedCar.DataAccess.GrpcClients.Interfaces;
using Middleware.RedCar.DataManagement.Interfaces;
using Middleware.RedCar.DataManagement.Models.Reservas;
using RedCar.Shared.Events.Reservas;
using RedCar.Shared.Messaging;

namespace Middleware.RedCar.Api.Messaging;

public sealed class ReservaSagaService : IReservaSagaService
{
    private readonly IBus _bus;
    private readonly ReservaSagaWaiter _waiter;
    private readonly EventBusSettings _settings;

    public ReservaSagaService(IBus bus, ReservaSagaWaiter waiter, IOptions<EventBusSettings> settings)
    {
        _bus = bus;
        _waiter = waiter;
        _settings = settings.Value;
    }

    public async Task<CrearReservaGrpcResult> CrearReservaViaEventBusAsync(
        CrearReservaGrpcRequest request,
        CancellationToken ct = default)
    {
        var correlationId = Guid.CreateVersion7();

        var cmd = new ProcesarReservaBookingCommand(
            correlationId,
            request.IdVehiculo,
            request.IdLocalizacionRecogida,
            request.IdLocalizacionDevolucion,
            request.FechaInicio.ToString("yyyy-MM-dd"),
            request.FechaFin.ToString("yyyy-MM-dd"),
            request.HoraInicio.ToString("HH:mm:ss"),
            request.HoraFin.ToString("HH:mm:ss"),
            request.Observaciones,
            request.OrigenCanalReserva,
            new ClienteBookingPayload(
                request.Cliente.Nombres, request.Cliente.Apellidos,
                request.Cliente.TipoIdentificacion, request.Cliente.NumeroIdentificacion,
                request.Cliente.Correo, request.Cliente.Telefono),
            request.Conductores.Select(c => new ConductorBookingPayload(
                c.IdConductor, c.Nombres, c.Apellidos, c.TipoIdentificacion, c.NumeroIdentificacion,
                c.FechaVencimientoLicencia.ToString("yyyy-MM-dd"), c.EdadConductor,
                c.Correo, c.Telefono, c.EsPrincipal)).ToList(),
            request.Extras.Select(e => new ExtraBookingPayload(e.IdExtra, e.Cantidad)).ToList());

        await _bus.Publish(cmd, ct);

        var outcome = await _waiter.WaitAsync(
            correlationId,
            TimeSpan.FromSeconds(_settings.SagaTimeoutSeconds),
            ct);

        if (!outcome.Success || outcome.Creada is null)
            throw new InvalidOperationException(outcome.MotivoRechazo ?? "No se pudo completar la reserva.");

        var p = outcome.Creada;
        return new CrearReservaGrpcResult(
            p.CodigoReserva,
            p.EstadoReserva,
            DateTimeOffset.Parse(p.FechaReservaUtc),
            p.CantidadDias,
            (decimal)p.SubtotalVehiculo,
            (decimal)p.SubtotalExtras,
            (decimal)p.Subtotal,
            (decimal)p.Iva,
            (decimal)p.Total);
    }
}
