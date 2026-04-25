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
    private readonly IUnitOfWork _unitOfWork;

    public PagoService(
        IPagoDataService pagoDataService,
        IReservaDataService reservaDataService,
        IUnitOfWork unitOfWork)
    {
        _pagoDataService = pagoDataService;
        _reservaDataService = reservaDataService;
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

        var created = await _pagoDataService.CreateAsync(model, usuario);
        await _unitOfWork.SaveChangesAsync();

        var result = await _pagoDataService.GetByIdAsync(created.IdPago);
        return MapToResponse(result!);
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
