using System.Globalization;
using Microsoft.Extensions.Logging;
using Middleware.RedCar.DataAccess.GrpcClients.Interfaces;
using Middleware.RedCar.DataAccess.Protos.Reservas;

namespace Middleware.RedCar.DataAccess.GrpcClients;

/// <summary>
/// Implementacion de IReservasGrpcClient que envuelve el cliente generado por Grpc.Tools
/// (ReservasGrpc.ReservasGrpcClient). Hace mapeo entre los DTOs publicos y los proto messages.
/// </summary>
public sealed class ReservasGrpcClient : IReservasGrpcClient
{
    private readonly ReservasGrpc.ReservasGrpcClient _client;
    private readonly ILogger<ReservasGrpcClient> _logger;

    public ReservasGrpcClient(ReservasGrpc.ReservasGrpcClient client, ILogger<ReservasGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<CrearReservaGrpcResult> CrearReservaAsync(CrearReservaGrpcRequest request, CancellationToken ct = default)
    {
        var proto = new CrearReservaRequest
        {
            IdVehiculo = request.IdVehiculo,
            IdLocalizacionRecogida = request.IdLocalizacionRecogida,
            IdLocalizacionDevolucion = request.IdLocalizacionDevolucion,
            FechaInicio = request.FechaInicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            FechaFin = request.FechaFin.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            HoraInicio = request.HoraInicio.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            HoraFin = request.HoraFin.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            Observaciones = request.Observaciones ?? string.Empty,
            OrigenCanalReserva = request.OrigenCanalReserva,
            IdCliente = request.IdCliente,
            Cliente = new ClienteDto
            {
                Nombres = request.Cliente.Nombres,
                Apellidos = request.Cliente.Apellidos,
                TipoIdentificacion = request.Cliente.TipoIdentificacion,
                NumeroIdentificacion = request.Cliente.NumeroIdentificacion,
                Correo = request.Cliente.Correo,
                Telefono = request.Cliente.Telefono
            }
        };

        foreach (var c in request.Conductores)
        {
            proto.Conductores.Add(new ConductorDto
            {
                IdConductor = c.IdConductor,
                Nombres = c.Nombres,
                Apellidos = c.Apellidos,
                TipoIdentificacion = c.TipoIdentificacion,
                NumeroIdentificacion = c.NumeroIdentificacion,
                FechaVencimientoLicencia = c.FechaVencimientoLicencia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EdadConductor = c.EdadConductor,
                Correo = c.Correo,
                Telefono = c.Telefono,
                EsPrincipal = c.EsPrincipal
            });
        }

        foreach (var e in request.Extras)
        {
            proto.Extras.Add(new ExtraDto
            {
                IdExtra = e.IdExtra,
                Cantidad = e.Cantidad
            });
        }

        _logger.LogInformation("gRPC CrearReserva vehiculo={Veh} loc={Loc} canal={Canal}",
            request.IdVehiculo, request.IdLocalizacionRecogida, request.OrigenCanalReserva);

        var resp = await _client.CrearReservaAsync(proto, cancellationToken: ct);

        return new CrearReservaGrpcResult(
            resp.CodigoReserva,
            resp.EstadoReserva,
            DateTimeOffset.Parse(resp.FechaReservaUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
            resp.CantidadDias,
            (decimal)resp.SubtotalVehiculo,
            (decimal)resp.SubtotalExtras,
            (decimal)resp.Subtotal,
            (decimal)resp.Iva,
            (decimal)resp.Total);
    }

    public async Task<CancelarReservaGrpcResult> CancelarReservaAsync(string codigoReserva, string motivo, string usuario, CancellationToken ct = default)
    {
        var proto = new CancelarReservaRequest
        {
            CodigoReserva = codigoReserva,
            MotivoCancelacion = motivo ?? string.Empty,
            UsuarioCancelacion = usuario ?? "BOOKING"
        };

        _logger.LogInformation("gRPC CancelarReserva {Codigo} motivo={Motivo}", codigoReserva, motivo);
        var resp = await _client.CancelarReservaAsync(proto, cancellationToken: ct);

        return new CancelarReservaGrpcResult(
            resp.CodigoReserva,
            resp.EstadoReserva,
            DateTimeOffset.Parse(resp.FechaCancelacionUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal));
    }
}
