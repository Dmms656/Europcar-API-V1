using System.Collections.Concurrent;
using System.Globalization;
using Grpc.Core;
using RedCar.Reservas.Api.Messaging;
using RedCar.Reservas.Api.Services;
using RedCar.Shared.Events;
using RedCar.Shared.Events.Reservas;
using RedCar.Shared.Protos.Reservas;
using ClienteDto = RedCar.Shared.Protos.Reservas.ClienteDto;
using ConductorDto = RedCar.Shared.Protos.Reservas.ConductorDto;
using ExtraDto = RedCar.Shared.Protos.Reservas.ExtraDto;

namespace RedCar.Reservas.Api.Messaging;

public sealed class ReservasBookingSagaService
{
    private static readonly ConcurrentDictionary<Guid, ProcesarReservaBookingCommand> Pending = new();

    private readonly ReservasWriteService _write;

    public ReservasBookingSagaService(ReservasWriteService write) => _write = write;

    public static void RegisterPending(ProcesarReservaBookingCommand cmd) =>
        Pending[cmd.CorrelationId] = cmd;

    public static ProcesarReservaBookingCommand? TakePending(Guid correlationId) =>
        Pending.TryRemove(correlationId, out var cmd) ? cmd : null;

    public async Task CompletarReservaAsync(
        ProcesarReservaBookingCommand cmd,
        ClienteActualizadoPayload cliente,
        CancellationToken ct)
    {
        var proto = new CrearReservaRequest
        {
            IdVehiculo = cmd.IdVehiculo,
            IdLocalizacionRecogida = cmd.IdLocalizacionRecogida,
            IdLocalizacionDevolucion = cmd.IdLocalizacionDevolucion,
            FechaInicio = cmd.FechaInicio,
            FechaFin = cmd.FechaFin,
            HoraInicio = cmd.HoraInicio,
            HoraFin = cmd.HoraFin,
            Observaciones = cmd.Observaciones ?? string.Empty,
            OrigenCanalReserva = cmd.OrigenCanalReserva,
            IdCliente = cliente.IdCliente,
            Cliente = new ClienteDto
            {
                Nombres = cmd.Cliente.Nombres,
                Apellidos = cmd.Cliente.Apellidos,
                TipoIdentificacion = cmd.Cliente.TipoIdentificacion,
                NumeroIdentificacion = cmd.Cliente.NumeroIdentificacion,
                Correo = cmd.Cliente.Correo,
                Telefono = cmd.Cliente.Telefono
            }
        };

        foreach (var c in cliente.Conductores)
        {
            proto.Conductores.Add(new ConductorDto
            {
                IdConductor = c.IdConductor,
                Nombres = c.Nombres,
                Apellidos = c.Apellidos,
                TipoIdentificacion = c.TipoIdentificacion,
                NumeroIdentificacion = c.NumeroIdentificacion,
                FechaVencimientoLicencia = c.FechaVencimientoLicencia,
                EdadConductor = c.EdadConductor,
                Correo = c.Correo,
                Telefono = c.Telefono,
                EsPrincipal = c.EsPrincipal
            });
        }

        foreach (var e in cmd.Extras)
            proto.Extras.Add(new ExtraDto { IdExtra = e.IdExtra, Cantidad = e.Cantidad });

        await _write.CrearReservaAsync(proto, ct, cmd.CorrelationId);
    }
}
