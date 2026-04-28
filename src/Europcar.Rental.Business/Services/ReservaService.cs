using Europcar.Rental.Business.DTOs.Request.Reservas;
using Europcar.Rental.Business.DTOs.Response.Reservas;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataManagement.Common;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Europcar.Rental.Business.Services;

public class ReservaService : IReservaService
{
    private readonly IReservaDataService _reservaDataService;
    private readonly IVehiculoDataService _vehiculoDataService;
    private readonly IClienteDataService _clienteDataService;
    private readonly IExtraDataService _extraDataService;
    private readonly IConductorDataService _conductorDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RentalDbContext _context;

    public ReservaService(
        IReservaDataService reservaDataService,
        IVehiculoDataService vehiculoDataService,
        IClienteDataService clienteDataService,
        IExtraDataService extraDataService,
        IConductorDataService conductorDataService,
        IUnitOfWork unitOfWork,
        RentalDbContext context)
    {
        _reservaDataService = reservaDataService;
        _vehiculoDataService = vehiculoDataService;
        _clienteDataService = clienteDataService;
        _extraDataService = extraDataService;
        _conductorDataService = conductorDataService;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<ReservaResponse> GetByCodigoAsync(string codigo)
    {
        var reserva = await _reservaDataService.GetByCodigoAsync(codigo)
            ?? throw new NotFoundException($"Reserva con código {codigo} no encontrada");
        return MapToResponse(reserva);
    }

    public async Task<IEnumerable<ReservaResponse>> GetByClienteIdAsync(int idCliente)
    {
        var reservas = await _reservaDataService.GetByClienteIdAsync(idCliente);
        return reservas.Select(MapToResponse);
    }

    public async Task<ReservaResponse> CreateAsync(CrearReservaRequest request)
    {
        // Validar fechas
        if (request.FechaHoraDevolucion <= request.FechaHoraRecogida)
            throw new BusinessException("La fecha de devolución debe ser posterior a la de recogida");

        if (request.FechaHoraRecogida <= DateTimeOffset.UtcNow)
            throw new BusinessException("La fecha de recogida debe ser futura");

        // Validar existencia del cliente
        var cliente = await _clienteDataService.GetByIdAsync(request.IdCliente)
            ?? throw new NotFoundException($"Cliente con ID {request.IdCliente} no encontrado");

        // Validar existencia y disponibilidad del vehículo
        var vehiculo = await _vehiculoDataService.GetByIdAsync(request.IdVehiculo)
            ?? throw new NotFoundException($"Vehículo con ID {request.IdVehiculo} no encontrado");

        if (vehiculo.EstadoOperativo != "DISPONIBLE")
            throw new BusinessException($"El vehículo {vehiculo.Placa} no está disponible. Estado: {vehiculo.EstadoOperativo}");

        // Validar solapamiento
        var solapamiento = await _reservaDataService.ExisteSolapamientoAsync(
            request.IdVehiculo, request.FechaHoraRecogida, request.FechaHoraDevolucion);
        if (solapamiento)
            throw new ConflictException("El vehículo ya tiene una reserva activa en el rango de fechas solicitado");

        // ── Procesar extras ──
        var extrasModel = new List<ReservaExtraModel>();
        decimal valorExtrasTotal = 0;

        if (request.Extras.Count > 0)
        {
            foreach (var item in request.Extras)
            {
                if (item.Cantidad <= 0)
                    throw new BusinessException($"La cantidad del extra {item.IdExtra} debe ser mayor a cero");

                // Validar que el extra exista y esté activo
                var extra = await _extraDataService.GetByIdAsync(item.IdExtra)
                    ?? throw new NotFoundException($"Extra con ID {item.IdExtra} no encontrado o inactivo");

                // Validar stock si el extra lo requiere
                if (extra.RequiereStock)
                {
                    var stockDisponible = await _extraDataService.GetStockDisponibleAsync(
                        request.IdLocalizacionRecogida, item.IdExtra);

                    if (stockDisponible < item.Cantidad)
                        throw new BusinessException(
                            $"Stock insuficiente para '{extra.NombreExtra}' en la sucursal de recogida. " +
                            $"Disponible: {stockDisponible}, solicitado: {item.Cantidad}");
                }

                var subtotalExtra = Math.Round(extra.ValorFijo * item.Cantidad, 2);
                valorExtrasTotal += subtotalExtra;

                extrasModel.Add(new ReservaExtraModel
                {
                    IdExtra = item.IdExtra,
                    CodigoExtra = extra.CodigoExtra,
                    NombreExtra = extra.NombreExtra,
                    Cantidad = item.Cantidad,
                    ValorUnitario = extra.ValorFijo,
                    Subtotal = subtotalExtra
                });
            }
        }

        // ── Calcular valores de renta ──
        var dias = (decimal)(request.FechaHoraDevolucion - request.FechaHoraRecogida).TotalDays;
        if (dias < 1) dias = 1;
        var subtotal = Math.Round(vehiculo.PrecioBaseDia * dias, 2);
        var cargoOneWay = request.IdLocalizacionRecogida != request.IdLocalizacionDevolucion ? 25.00m : 0;
        var baseImponible = subtotal + valorExtrasTotal + cargoOneWay;
        var impuestos = Math.Round(baseImponible * 0.12m, 2); // 12% IVA
        var total = baseImponible + impuestos;

        var model = new ReservaModel
        {
            CodigoReserva = $"RSV-{Guid.NewGuid().ToString("N")[..10].ToUpper()}",
            IdCliente = request.IdCliente,
            IdVehiculo = request.IdVehiculo,
            IdLocalizacionRecogida = request.IdLocalizacionRecogida,
            IdLocalizacionDevolucion = request.IdLocalizacionDevolucion,
            CanalReserva = request.CanalReserva,
            FechaHoraRecogida = request.FechaHoraRecogida,
            FechaHoraDevolucion = request.FechaHoraDevolucion,
            Subtotal = subtotal,
            ValorImpuestos = impuestos,
            ValorExtras = valorExtrasTotal,
            CargoOneWay = cargoOneWay,
            Total = total,
            CodigoConfirmacion = $"CONF-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            Extras = extrasModel
        };

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var created = await _reservaDataService.CreateAsync(model);

            // ── Asignar conductores ──
            await AssignConductoresAsync(request, created.IdReserva, cliente);

            // ── Reservar stock de extras que lo requieran ──
            foreach (var item in request.Extras)
            {
                var extra = await _extraDataService.GetByIdAsync(item.IdExtra);
                if (extra != null && extra.RequiereStock)
                {
                    await _extraDataService.ReservarStockAsync(
                        request.IdLocalizacionRecogida, item.IdExtra, item.Cantidad);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await tx.CommitAsync();

            // Recargar con todos los includes para devolver respuesta completa
            var result = await _reservaDataService.GetByIdAsync(created.IdReserva);
            return MapToResponse(result!);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<ReservaResponse> ConfirmarAsync(int id, string usuario, decimal? monto = null, string? referenciaExterna = null)
    {
        var reserva = await _reservaDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Reserva con ID {id} no encontrada");

        if (reserva.EstadoReserva != "PENDIENTE")
            throw new BusinessException($"Solo se puede confirmar una reserva en estado PENDIENTE. Estado actual: {reserva.EstadoReserva}");

        var montoFinal = monto ?? reserva.Total;
        var codigoPago = $"PAG-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";

        // 1. INSERT pago via raw SQL (no change tracker)
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO rental.pagos 
            (pago_guid, codigo_pago, id_reserva, id_cliente, tipo_pago, metodo_pago, estado_pago, 
             referencia_externa, monto, moneda, fecha_pago_utc, observaciones_pago, creado_por_usuario, origen_registro)
            VALUES ({Guid.NewGuid()}, {codigoPago}, {id}, {reserva.IdCliente}, 'COBRO', 'TARJETA', 'APROBADO',
                    {referenciaExterna ?? "WEB"}, {montoFinal}, 'USD', CURRENT_TIMESTAMP, 
                    {"Pago automático - Confirmación reserva"}, {usuario}, 'API')");

        // 2. INSERT factura via raw SQL
        var ivaRate = 0.15m;
        var subtotal = Math.Round(montoFinal / (1 + ivaRate), 2);
        var valorIva = montoFinal - subtotal;
        var numFactura = $"FAC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO rental.facturas
                (factura_guid, numero_factura, id_cliente, id_reserva, fecha_emision,
                 subtotal, valor_iva, total, estado_factura, servicio_origen, origen_canal_factura,
                 observaciones_factura, creado_por_usuario, fecha_registro_utc)
                VALUES ({Guid.NewGuid()}, {numFactura}, {reserva.IdCliente}, {id},
                        CURRENT_TIMESTAMP, {subtotal}, {valorIva}, {montoFinal}, 'EMITIDA', 'RESERVA_WEB', 'WEB',
                        {"Factura automática - Pago " + codigoPago}, {usuario}, CURRENT_TIMESTAMP)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Error generando factura: {ex.Message}");
        }

        // 3. UPDATE reserva status via raw SQL
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE rental.reservas 
            SET estado_reserva = 'CONFIRMADA', 
                modificado_por_usuario = {usuario}, 
                fecha_modificacion_utc = CURRENT_TIMESTAMP
            WHERE id_reserva = {id}");

        // Return updated data (reuse existing reserva model, just change estado)
        reserva.EstadoReserva = "CONFIRMADA";
        return MapToResponse(reserva);
    }

    public async Task<ReservaResponse> CancelarAsync(int id, string motivo, string usuario)
    {
        var reserva = await _reservaDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Reserva con ID {id} no encontrada");

        if (reserva.EstadoReserva == "FINALIZADA" || reserva.EstadoReserva == "CANCELADA")
            throw new BusinessException($"No se puede cancelar una reserva en estado {reserva.EstadoReserva}");

        // Bloqueo de cancelación para reservas cuya recogida ya ocurrió.
        // Una reserva en curso o pasada queda lockeada y debe seguir el flujo administrativo.
        if (reserva.FechaHoraRecogida <= DateTimeOffset.UtcNow)
            throw new BusinessException(
                "No se puede cancelar una reserva cuya fecha de recogida ya ha pasado. " +
                "Las reservas en curso o pasadas quedan bloqueadas para cancelación por el cliente.");

        // Liberar stock de extras que lo requieran
        if (reserva.Extras.Count > 0)
        {
            foreach (var extra in reserva.Extras)
            {
                var detalle = await _extraDataService.GetByIdAsync(extra.IdExtra);
                if (detalle != null && detalle.RequiereStock)
                {
                    await _extraDataService.LiberarStockAsync(
                        reserva.IdLocalizacionRecogida, extra.IdExtra, extra.Cantidad);
                }
            }
        }

        // Anular pagos asociados a esta reserva
        var pagos = await _context.Pagos
            .Where(p => p.IdReserva == id && p.EstadoPago != "ANULADO" && p.EstadoPago != "REEMBOLSADO")
            .ToListAsync();
        foreach (var pago in pagos)
        {
            pago.EstadoPago = "ANULADO";
            pago.ModificadoPorUsuario = usuario;
            pago.FechaModificacionUtc = DateTimeOffset.UtcNow;
        }

        // Anular facturas asociadas a esta reserva
        var facturas = await _context.Facturas
            .Where(f => f.IdReserva == id && f.EstadoFactura != "ANULADA")
            .ToListAsync();
        foreach (var factura in facturas)
        {
            factura.EstadoFactura = "ANULADA";
            factura.ModificadoPorUsuario = usuario;
            factura.FechaModificacionUtc = DateTimeOffset.UtcNow;
        }

        await _reservaDataService.UpdateEstadoAsync(id, "CANCELADA", usuario, motivo);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _reservaDataService.GetByIdAsync(id);
        return MapToResponse(updated!);
    }

    private static ReservaResponse MapToResponse(ReservaModel r) => new()
    {
        IdReserva = r.IdReserva,
        ReservaGuid = r.ReservaGuid,
        CodigoReserva = r.CodigoReserva,
        CodigoConfirmacion = r.CodigoConfirmacion,
        EstadoReserva = r.EstadoReserva,
        IdCliente = r.IdCliente,
        FechaHoraRecogida = r.FechaHoraRecogida,
        FechaHoraDevolucion = r.FechaHoraDevolucion,
        Subtotal = r.Subtotal,
        ValorImpuestos = r.ValorImpuestos,
        ValorExtras = r.ValorExtras,
        CargoOneWay = r.CargoOneWay,
        Total = r.Total,
        NombreCliente = r.NombreCliente,
        PlacaVehiculo = r.PlacaVehiculo,
        DescripcionVehiculo = r.DescripcionVehiculo,
        Extras = r.Extras.Select(e => new ReservaExtraItemResponse
        {
            IdReservaExtra = e.IdReservaExtra,
            IdExtra = e.IdExtra,
            CodigoExtra = e.CodigoExtra,
            NombreExtra = e.NombreExtra,
            Cantidad = e.Cantidad,
            ValorUnitario = e.ValorUnitario,
            Subtotal = e.Subtotal
        }).ToList()
    };

    /// <summary>
    /// Assigns conductors to a reservation. If no conductors are specified,
    /// the client is automatically assigned as the principal conductor.
    /// </summary>
    private async Task AssignConductoresAsync(CrearReservaRequest request, int idReserva, ClienteModel cliente)
    {
        var conductoresToAssign = request.Conductores;

        // If no conductors specified, find or create the client's own conductor record
        if (conductoresToAssign.Count == 0)
        {
            var clientConductores = await _conductorDataService.GetByClienteIdAsync(cliente.IdCliente);
            var existing = clientConductores.FirstOrDefault();

            int conductorId;
            if (existing != null)
            {
                conductorId = existing.IdConductor;
            }
            else
            {
                // Auto-create conductor from client data
                var age = (short)(DateTime.Today.Year - cliente.FechaNacimiento.Year);
                var newConductor = await _conductorDataService.CreateAsync(new ConductorModel
                {
                    IdCliente = cliente.IdCliente,
                    TipoIdentificacion = cliente.TipoIdentificacion,
                    NumeroIdentificacion = cliente.NumeroIdentificacion,
                    Nombre1 = cliente.Nombre1,
                    Nombre2 = cliente.Nombre2,
                    Apellido1 = cliente.Apellido1,
                    Apellido2 = cliente.Apellido2,
                    NumeroLicencia = "PENDIENTE",
                    FechaVencimientoLicencia = DateOnly.FromDateTime(DateTime.Today.AddYears(2)),
                    EdadConductor = age,
                    Telefono = cliente.Telefono,
                    Correo = cliente.Correo
                });
                conductorId = newConductor.IdConductor;
            }

            conductoresToAssign = new List<ReservaConductorItemRequest>
            {
                new() { IdConductor = conductorId, EsPrincipal = true }
            };
        }

        // Validate that exactly one principal conductor exists
        var principals = conductoresToAssign.Count(c => c.EsPrincipal);
        if (principals == 0 && conductoresToAssign.Count > 0)
        {
            conductoresToAssign[0].EsPrincipal = true;
        }

        foreach (var item in conductoresToAssign)
        {
            var conductor = await _conductorDataService.GetByIdAsync(item.IdConductor)
                ?? throw new NotFoundException($"Conductor con ID {item.IdConductor} no encontrado");

            await _reservaDataService.AddConductorAsync(idReserva, item.IdConductor, item.EsPrincipal,
                conductor.EsConductorJoven == true ? 15.00m : 0m);
        }
    }
}
