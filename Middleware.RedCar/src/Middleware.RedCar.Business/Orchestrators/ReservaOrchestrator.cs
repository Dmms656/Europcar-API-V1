using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.Business.DTOs.Reservas;
using Middleware.RedCar.Business.Exceptions;
using Middleware.RedCar.Business.Interfaces;
using Middleware.RedCar.Business.Mappers;
using Middleware.RedCar.DataAccess.GrpcClients.Interfaces;
using Middleware.RedCar.DataManagement.Interfaces;

namespace Middleware.RedCar.Business.Orchestrators;

public sealed class ReservaOrchestrator : IReservaOrchestrator
{
    private readonly ICatalogoDataService _catalogo;
    private readonly IClientesDataService _clientes;
    private readonly IReservasDataService _reservas;
    private readonly NegocioSettings _negocio;
    private readonly ILogger<ReservaOrchestrator> _logger;

    public ReservaOrchestrator(
        ICatalogoDataService catalogo,
        IClientesDataService clientes,
        IReservasDataService reservas,
        IOptions<NegocioSettings> negocio,
        ILogger<ReservaOrchestrator> logger)
    {
        _catalogo = catalogo;
        _clientes = clientes;
        _reservas = reservas;
        _negocio = negocio.Value;
        _logger = logger;
    }

    public async Task<DisponibilidadResponse> VerificarDisponibilidadAsync(int idVehiculo, int idLocalizacion, DateTimeOffset fechaRecogida, DateTimeOffset fechaDevolucion, CancellationToken ct = default)
    {
        if (fechaDevolucion <= fechaRecogida)
            throw new ValidationException(new[] { new ValidationFailure("fechaDevolucion", "fechaDevolucion debe ser posterior a fechaRecogida.") });

        var disponible = await _reservas.VerificarDisponibilidadAsync(idVehiculo, idLocalizacion, fechaRecogida, fechaDevolucion, ct);

        return new DisponibilidadResponse
        {
            IdVehiculo = idVehiculo,
            IdLocalizacion = idLocalizacion,
            Disponibilidad = new DisponibilidadDetalle
            {
                FechaRecogida = fechaRecogida,
                FechaDevolucion = fechaDevolucion,
                Disponible = disponible
            }
        };
    }

    public async Task<CrearReservaBookingResponse> CrearReservaAsync(CrearReservaBookingRequest request, CancellationToken ct = default)
    {
        // 1. Validar vehiculo existe + recuperar datos para el response
        var vehiculo = await _catalogo.GetVehiculoAsync(request.IdVehiculo, ct)
            ?? throw new NotFoundException($"Vehiculo {request.IdVehiculo} no existe.");

        // 2. Comprobar disponibilidad final justo antes de confirmar (anti-race)
        var fechaRecogida = request.FechaInicio.ToDateTime(request.HoraInicio, DateTimeKind.Utc);
        var fechaDevolucion = request.FechaFin.ToDateTime(request.HoraFin, DateTimeKind.Utc);

        var disponible = await _reservas.VerificarDisponibilidadAsync(
            request.IdVehiculo, request.IdLocalizacionRecogida,
            new DateTimeOffset(fechaRecogida, TimeSpan.Zero),
            new DateTimeOffset(fechaDevolucion, TimeSpan.Zero), ct);

        if (!disponible)
            throw new ConflictException("El vehiculo ya no esta disponible para esas fechas.");

        // 3. Upsert cliente + conductores en MS.Clientes
        var clienteUpsertReq = ClientesBusinessMapper.ToUpsert(request.Cliente);
        var cliente = await _clientes.UpsertClienteAsync(clienteUpsertReq, ct);

        var conductoresUpsert = request.Conductores.Select(ClientesBusinessMapper.ToUpsert).ToList();
        var conductores = await _clientes.UpsertConductoresAsync(cliente.IdCliente, conductoresUpsert, ct);

        _logger.LogInformation("Cliente {Id} y {Count} conductores listos para reserva {Vehiculo}",
            cliente.IdCliente, conductores.Count, request.IdVehiculo);

        // 4. Crear reserva via gRPC (operacion transaccional en MS.Reservas)
        var grpcReq = new CrearReservaGrpcRequest(
            IdVehiculo: request.IdVehiculo,
            IdLocalizacionRecogida: request.IdLocalizacionRecogida,
            IdLocalizacionDevolucion: request.IdLocalizacionDevolucion,
            FechaInicio: request.FechaInicio,
            FechaFin: request.FechaFin,
            HoraInicio: request.HoraInicio,
            HoraFin: request.HoraFin,
            Observaciones: request.Observaciones,
            OrigenCanalReserva: _negocio.OrigenCanalReserva,
            IdCliente: cliente.IdCliente,
            Cliente: new CrearReservaGrpcCliente(
                cliente.Nombres, cliente.Apellidos,
                cliente.TipoIdentificacion, cliente.NumeroIdentificacion,
                cliente.Correo, cliente.Telefono),
            Conductores: conductores.Select(c => new CrearReservaGrpcConductor(
                c.IdConductor,
                c.Nombres, c.Apellidos,
                c.TipoIdentificacion, c.NumeroIdentificacion,
                c.FechaVencimientoLicencia, c.EdadConductor,
                c.Correo, c.Telefono,
                c.EsPrincipal)).ToList(),
            Extras: request.Extras.Select(e => new CrearReservaGrpcExtra(e.IdExtra, e.Cantidad)).ToList());

        var resultado = await _reservas.CrearReservaAsync(grpcReq, ct);

        // 5. Armar response del contrato
        return ReservasBusinessMapper.ToBooking(
            resultado,
            request.FechaInicio, request.FechaFin,
            request.HoraInicio, request.HoraFin,
            new VehiculoReservaResumen
            {
                IdVehiculo = vehiculo.IdVehiculo,
                CodigoInterno = vehiculo.CodigoInterno,
                Marca = vehiculo.Marca,
                Modelo = vehiculo.Modelo
            });
    }

    public async Task<ReservaBookingResponse> GetReservaAsync(string codigoReserva, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigoReserva))
            throw new ValidationException(new[] { new ValidationFailure("codigoReserva", "codigoReserva es obligatorio.") });

        var data = await _reservas.GetReservaAsync(codigoReserva, ct)
            ?? throw new NotFoundException($"Reserva {codigoReserva} no encontrada.");

        return ReservasBusinessMapper.ToBooking(data);
    }

    public async Task<CancelarReservaResponse> CancelarReservaAsync(string codigoReserva, CancelarReservaRequest request, string? usuarioCancelacion, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigoReserva))
            throw new ValidationException(new[] { new ValidationFailure("codigoReserva", "codigoReserva es obligatorio.") });

        var resultado = await _reservas.CancelarReservaAsync(
            codigoReserva,
            request.MotivoCancelacion,
            usuarioCancelacion ?? "BOOKING",
            ct);

        return new CancelarReservaResponse
        {
            CodigoReserva = resultado.CodigoReserva,
            EstadoReserva = resultado.EstadoReserva,
            FechaCancelacionUtc = resultado.FechaCancelacionUtc,
            MotivoCancelacion = request.MotivoCancelacion
        };
    }
}
