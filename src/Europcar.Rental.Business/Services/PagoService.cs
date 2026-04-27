using Europcar.Rental.Business.DTOs.Request.Pagos;
using Europcar.Rental.Business.DTOs.Response.Pagos;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Common;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.Business.Services;

public class PagoService : IPagoService
{
    private readonly IPagoDataService _pagoDataService;
    private readonly IReservaDataService _reservaDataService;
    private readonly IFacturaDataService _facturaDataService;
    private readonly IUnitOfWork _unitOfWork;

    public PagoService(
        IPagoDataService pagoDataService,
        IReservaDataService reservaDataService,
        IFacturaDataService facturaDataService,
        IUnitOfWork unitOfWork)
    {
        _pagoDataService = pagoDataService;
        _reservaDataService = reservaDataService;
        _facturaDataService = facturaDataService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagoResponse> GetByIdAsync(int id)
    {
        var pago = await _pagoDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Pago con ID {id} no encontrado");
        return MapToResponse(pago);
    }

    public async Task<IEnumerable<PagoResponse>> GetByReservaIdAsync(int idReserva)
    {
        var pagos = await _pagoDataService.GetByReservaIdAsync(idReserva);
        return pagos.Select(MapToResponse);
    }

    public async Task<PagoResponse> CreateAsync(CrearPagoRequest request, string usuario)
    {
        if (request.IdReserva == null && request.IdContrato == null)
            throw new BusinessException("Todo pago debe referenciar una reserva o un contrato");

        if (request.Monto <= 0)
            throw new BusinessException("El monto del pago debe ser mayor a cero");

        var codigo = $"PAG-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";

        // === Build ALL entities in the context first, THEN single SaveChanges ===

        // 1. Add Pago to context (NO save yet)
        var model = new PagoModel
        {
            CodigoPago = codigo,
            IdReserva = request.IdReserva,
            IdContrato = request.IdContrato,
            IdCliente = request.IdCliente,
            TipoPago = request.TipoPago,
            MetodoPago = request.MetodoPago,
            EstadoPago = "APROBADO",
            ReferenciaExterna = request.ReferenciaExterna,
            Monto = request.Monto,
            ObservacionesPago = request.Observaciones
        };
        await _pagoDataService.AddAsync(model, usuario);

        // 2. Add Factura to context (NO save yet)
        var ivaRate = 0.15m;
        var subtotal = Math.Round(request.Monto / (1 + ivaRate), 2);
        var valorIva = request.Monto - subtotal;
        await _facturaDataService.AddAsync(new FacturaModel
        {
            NumeroFactura = $"FAC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            IdCliente = request.IdCliente,
            IdReserva = request.IdReserva,
            IdContrato = request.IdContrato,
            Subtotal = subtotal,
            ValorIva = valorIva,
            Total = request.Monto,
            EstadoFactura = "EMITIDA",
            ServicioOrigen = "RESERVA_WEB",
            OrigenCanalFactura = "WEB",
            ObservacionesFactura = $"Factura automática - Pago {codigo}",
        }, usuario);

        // 3. Save pago + factura in one DB roundtrip
        await _unitOfWork.SaveChangesAsync();

        // 4. Update reserva status via direct SQL (ExecuteUpdateAsync - bypasses change tracker)
        if (request.IdReserva.HasValue)
        {
            try
            {
                await _reservaDataService.UpdateEstadoAsync(request.IdReserva.Value, "CONFIRMADA", usuario);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Error confirmando reserva: {ex.Message}");
            }
        }

        return new PagoResponse
        {
            IdPago = model.IdPago,
            PagoGuid = model.PagoGuid,
            CodigoPago = codigo,
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

    private static PagoResponse MapToResponse(PagoModel p) => new()
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
        NombreCliente = p.NombreCliente,
        CodigoReserva = p.CodigoReserva,
        ObservacionesPago = p.ObservacionesPago
    };
}
