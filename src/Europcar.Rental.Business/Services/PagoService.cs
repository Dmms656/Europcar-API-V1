using Europcar.Rental.Business.DTOs.Request.Pagos;
using Europcar.Rental.Business.DTOs.Response.Pagos;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace Europcar.Rental.Business.Services;

public class PagoService : IPagoService
{
    private readonly RentalDbContext _context;

    public PagoService(RentalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PagoResponse>> GetAllAsync()
    {
        return await _context.Pagos
            .Include(p => p.Cliente)
            .Include(p => p.Reserva)
            .OrderByDescending(p => p.FechaPagoUtc)
            .Select(p => new PagoResponse
            {
                IdPago = p.IdPago,
                PagoGuid = p.PagoGuid,
                CodigoPago = p.CodigoPago,
                TipoPago = p.TipoPago,
                MetodoPago = p.MetodoPago,
                EstadoPago = p.EstadoPago,
                Monto = p.Monto,
                Moneda = p.Moneda,
                FechaPagoUtc = p.FechaPagoUtc,
                ReferenciaExterna = p.ReferenciaExterna,
                NombreCliente = p.Cliente != null ? p.Cliente.CliNombre1 + " " + p.Cliente.CliApellido1 : null,
                CodigoReserva = p.Reserva != null ? p.Reserva.CodigoReserva : null,
                ObservacionesPago = p.ObservacionesPago
            })
            .ToListAsync();
    }

    public async Task<PagoResponse> GetByIdAsync(int id)
    {
        var p = await _context.Pagos
            .Include(p => p.Cliente)
            .Include(p => p.Reserva)
            .FirstOrDefaultAsync(p => p.IdPago == id)
            ?? throw new NotFoundException($"Pago con ID {id} no encontrado");

        return new PagoResponse
        {
            IdPago = p.IdPago,
            PagoGuid = p.PagoGuid,
            CodigoPago = p.CodigoPago,
            TipoPago = p.TipoPago,
            MetodoPago = p.MetodoPago,
            EstadoPago = p.EstadoPago,
            Monto = p.Monto,
            Moneda = p.Moneda,
            FechaPagoUtc = p.FechaPagoUtc,
            ReferenciaExterna = p.ReferenciaExterna,
            NombreCliente = p.Cliente != null ? $"{p.Cliente.CliNombre1} {p.Cliente.CliApellido1}" : null,
            CodigoReserva = p.Reserva?.CodigoReserva,
            ObservacionesPago = p.ObservacionesPago
        };
    }

    public async Task<IEnumerable<PagoResponse>> GetByReservaIdAsync(int idReserva)
    {
        return await _context.Pagos
            .Include(p => p.Cliente)
            .Include(p => p.Reserva)
            .Where(p => p.IdReserva == idReserva)
            .OrderByDescending(p => p.FechaPagoUtc)
            .Select(p => new PagoResponse
            {
                IdPago = p.IdPago,
                PagoGuid = p.PagoGuid,
                CodigoPago = p.CodigoPago,
                TipoPago = p.TipoPago,
                MetodoPago = p.MetodoPago,
                EstadoPago = p.EstadoPago,
                Monto = p.Monto,
                Moneda = p.Moneda,
                FechaPagoUtc = p.FechaPagoUtc,
                ReferenciaExterna = p.ReferenciaExterna,
                ObservacionesPago = p.ObservacionesPago
            })
            .ToListAsync();
    }

    public async Task<PagoResponse> CreateAsync(CrearPagoRequest request, string usuario)
    {
        if (request.IdReserva == null && request.IdContrato == null)
            throw new BusinessException("Todo pago debe referenciar una reserva o un contrato");

        if (request.Monto <= 0)
            throw new BusinessException("El monto del pago debe ser mayor a cero");

        var codigoPago = $"PAG-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";
        var pagoGuid = Guid.NewGuid();
        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. INSERT pago
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO rental.pagos 
                (pago_guid, codigo_pago, id_reserva, id_contrato, id_cliente, tipo_pago, metodo_pago, estado_pago, 
                 referencia_externa, monto, moneda, fecha_pago_utc, observaciones_pago, creado_por_usuario, origen_registro)
                VALUES ({pagoGuid}, {codigoPago}, {request.IdReserva}, {request.IdContrato}, {request.IdCliente}, 
                        {request.TipoPago}, {request.MetodoPago}, 'APROBADO',
                        {request.ReferenciaExterna}, {request.Monto}, 'USD', CURRENT_TIMESTAMP, {request.Observaciones}, 
                        {usuario}, 'API')");

            var ivaRate = 0.15m;
            var subtotal = Math.Round(request.Monto / (1 + ivaRate), 2);
            var valorIva = request.Monto - subtotal;
            var numFactura = $"FAC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            // 2. INSERT factura
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO rental.facturas
                (factura_guid, numero_factura, id_cliente, id_reserva, id_contrato, fecha_emision,
                 subtotal, valor_iva, total, estado_factura, servicio_origen, origen_canal_factura,
                 observaciones_factura, creado_por_usuario, fecha_registro_utc)
                VALUES ({Guid.NewGuid()}, {numFactura}, {request.IdCliente}, {request.IdReserva}, {request.IdContrato},
                        CURRENT_TIMESTAMP, {subtotal}, {valorIva}, {request.Monto}, 'EMITIDA', 'RESERVA_WEB', 'WEB',
                        {"Factura automática - Pago " + codigoPago}, {usuario}, CURRENT_TIMESTAMP)");

            // 3. UPDATE reserva estado
            if (request.IdReserva.HasValue)
            {
                var updated = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE rental.reservas 
                    SET estado_reserva = 'CONFIRMADA', 
                        modificado_por_usuario = {usuario}, 
                        fecha_modificacion_utc = CURRENT_TIMESTAMP
                    WHERE id_reserva = {request.IdReserva.Value}");

                if (updated == 0)
                    throw new BusinessException($"No se pudo confirmar la reserva {request.IdReserva.Value}.");
            }

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        var idPago = await _context.Pagos
            .AsNoTracking()
            .Where(p => p.PagoGuid == pagoGuid)
            .Select(p => p.IdPago)
            .FirstOrDefaultAsync();

        return new PagoResponse
        {
            IdPago = idPago,
            PagoGuid = pagoGuid,
            CodigoPago = codigoPago,
            TipoPago = request.TipoPago,
            MetodoPago = request.MetodoPago,
            EstadoPago = "APROBADO",
            Monto = request.Monto,
            Moneda = "USD",
            FechaPagoUtc = DateTimeOffset.UtcNow,
            ReferenciaExterna = request.ReferenciaExterna,
            ObservacionesPago = request.Observaciones
        };
    }
}
