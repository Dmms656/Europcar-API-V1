using Europcar.Rental.Business.DTOs.Request.Contratos;
using Europcar.Rental.Business.DTOs.Response.Contratos;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataManagement.Common;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Europcar.Rental.Business.Services;

public class ContratoService : IContratoService
{
    private readonly IContratoDataService _contratoDataService;
    private readonly ICheckInOutDataService _checkInOutDataService;
    private readonly IReservaDataService _reservaDataService;
    private readonly IVehiculoDataService _vehiculoDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RentalDbContext _context;

    public ContratoService(
        IContratoDataService contratoDataService,
        ICheckInOutDataService checkInOutDataService,
        IReservaDataService reservaDataService,
        IVehiculoDataService vehiculoDataService,
        IUnitOfWork unitOfWork,
        RentalDbContext context)
    {
        _contratoDataService = contratoDataService;
        _checkInOutDataService = checkInOutDataService;
        _reservaDataService = reservaDataService;
        _vehiculoDataService = vehiculoDataService;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<ContratoResponse> GetByIdAsync(int id)
    {
        var contrato = await _contratoDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Contrato con ID {id} no encontrado");
        return MapToResponse(contrato);
    }

    public async Task<IEnumerable<ContratoResponse>> GetAllAsync()
    {
        var contratos = await _contratoDataService.GetAllAsync();
        return contratos.Select(MapToResponse);
    }

    public async Task<IEnumerable<ContratoResponse>> GetByClienteIdAsync(int idCliente)
    {
        var contratos = await _contratoDataService.GetByClienteIdAsync(idCliente);
        return contratos.Select(MapToResponse);
    }

    public async Task<ContratoResponse> CrearDesdeReservaAsync(CrearContratoRequest request, string usuario)
    {
        var reserva = await _reservaDataService.GetByIdAsync(request.IdReserva)
            ?? throw new NotFoundException($"Reserva con ID {request.IdReserva} no encontrada");

        if (reserva.EstadoReserva != "CONFIRMADA")
            throw new BusinessException("Solo se puede generar contrato para reservas confirmadas");

        // Verificar que no exista ya un contrato para esta reserva
        var existente = await _contratoDataService.GetByReservaIdAsync(request.IdReserva);
        if (existente != null)
            throw new ConflictException("Ya existe un contrato para esta reserva");

        var numero = $"CTR-{DateTime.UtcNow:yyyyMMddHHmmss}-{request.IdReserva:D6}";

        var model = new ContratoModel
        {
            NumeroContrato = numero,
            IdReserva = reserva.IdReserva,
            IdCliente = reserva.IdCliente,
            IdVehiculo = reserva.IdVehiculo,
            FechaHoraSalida = reserva.FechaHoraRecogida,
            FechaHoraPrevistaDevolucion = reserva.FechaHoraDevolucion,
            KilometrajeSalida = request.KilometrajeSalida,
            NivelCombustibleSalida = request.NivelCombustibleSalida,
            ObservacionesContrato = request.Observaciones
        };

        var created = await _contratoDataService.CreateAsync(model, usuario);

        // El trigger fn_post_contrato_creado cambia la reserva a EN_CURSO y el vehículo a ALQUILADO
        await _unitOfWork.SaveChangesAsync();

        var result = await _contratoDataService.GetByIdAsync(created.IdContrato);
        return MapToResponse(result!);
    }

    public async Task<CheckInOutResponse> RegistrarCheckOutAsync(CheckOutRequest request, string usuario)
    {
        var contrato = await _contratoDataService.GetByIdAsync(request.IdContrato)
            ?? throw new NotFoundException($"Contrato con ID {request.IdContrato} no encontrado");

        if (contrato.EstadoContrato != "ABIERTO")
            throw new BusinessException("El contrato debe estar abierto para registrar check-out");

        var model = new CheckInOutModel
        {
            IdContrato = request.IdContrato,
            TipoCheck = "CHECKOUT",
            FechaHoraCheck = DateTimeOffset.UtcNow,
            Kilometraje = request.Kilometraje,
            NivelCombustible = request.NivelCombustible,
            Limpio = request.Limpio,
            Observaciones = request.Observaciones
        };

        var created = await _checkInOutDataService.CreateAsync(model, usuario);
        await _unitOfWork.SaveChangesAsync();

        return MapToCheckResponse(created);
    }

    public async Task<ContratoResponse> UpdateAsync(int id, ActualizarContratoRequest request, string usuario)
    {
        var entity = await _context.Contratos.FirstOrDefaultAsync(c => c.IdContrato == id)
            ?? throw new NotFoundException($"Contrato con ID {id} no encontrado");

        if (entity.EstadoContrato == "CERRADO")
            throw new BusinessException("No se puede editar un contrato cerrado");
        if (request.FechaHoraPrevistaDevolucion <= request.FechaHoraSalida)
            throw new BusinessException("La fecha prevista de devolución debe ser posterior a la fecha de salida");

        entity.FechaHoraSalida = request.FechaHoraSalida;
        entity.FechaHoraPrevistaDevolucion = request.FechaHoraPrevistaDevolucion;
        entity.KilometrajeSalida = request.KilometrajeSalida;
        entity.NivelCombustibleSalida = request.NivelCombustibleSalida;
        entity.EstadoContrato = request.EstadoContrato.Trim().ToUpperInvariant();
        entity.ObservacionesContrato = string.IsNullOrWhiteSpace(request.Observaciones) ? null : request.Observaciones.Trim();
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
        var updated = await _contratoDataService.GetByIdAsync(id);
        return MapToResponse(updated!);
    }

    public async Task<CheckInOutResponse> RegistrarCheckInAsync(CheckInRequest request, string usuario)
    {
        var contrato = await _contratoDataService.GetByIdAsync(request.IdContrato)
            ?? throw new NotFoundException($"Contrato con ID {request.IdContrato} no encontrado");

        if (contrato.EstadoContrato != "ABIERTO")
            throw new BusinessException("El contrato debe estar abierto para registrar check-in (devolución)");

        // Calcular cargos adicionales
        decimal cargoCombustible = 0;
        decimal cargoLimpieza = 0;
        decimal cargoKmExtra = 0;

        // Cargo por combustible faltante: diferencia * $3.50/% (tarifa configurable)
        if (request.NivelCombustible < contrato.NivelCombustibleSalida)
        {
            var diferencia = contrato.NivelCombustibleSalida - request.NivelCombustible;
            cargoCombustible = Math.Round(diferencia * 3.50m, 2);
        }

        // Cargo por limpieza
        if (!request.Limpio)
        {
            cargoLimpieza = 25.00m;
        }

        var model = new CheckInOutModel
        {
            IdContrato = request.IdContrato,
            TipoCheck = "CHECKIN",
            FechaHoraCheck = DateTimeOffset.UtcNow,
            Kilometraje = request.Kilometraje,
            NivelCombustible = request.NivelCombustible,
            Limpio = request.Limpio,
            Observaciones = request.Observaciones,
            CargoCombustible = cargoCombustible,
            CargoLimpieza = cargoLimpieza,
            CargoKmExtra = cargoKmExtra
        };

        // El trigger fn_post_checkin_checkout cierra el contrato, finaliza la reserva
        // y devuelve el vehículo a DISPONIBLE
        var created = await _checkInOutDataService.CreateAsync(model, usuario);
        await _unitOfWork.SaveChangesAsync();

        return MapToCheckResponse(created);
    }

    private static ContratoResponse MapToResponse(ContratoModel c) => new()
    {
        IdContrato = c.IdContrato,
        ContratoGuid = c.ContratoGuid,
        NumeroContrato = c.NumeroContrato,
        EstadoContrato = c.EstadoContrato,
        FechaHoraSalida = c.FechaHoraSalida,
        FechaHoraPrevistaDevolucion = c.FechaHoraPrevistaDevolucion,
        KilometrajeSalida = c.KilometrajeSalida,
        NivelCombustibleSalida = c.NivelCombustibleSalida,
        NombreCliente = c.NombreCliente,
        PlacaVehiculo = c.PlacaVehiculo,
        CodigoReserva = c.CodigoReserva,
        ObservacionesContrato = c.ObservacionesContrato
    };

    private static CheckInOutResponse MapToCheckResponse(CheckInOutModel c) => new()
    {
        IdCheck = c.IdCheck,
        CheckGuid = c.CheckGuid,
        TipoCheck = c.TipoCheck,
        FechaHoraCheck = c.FechaHoraCheck,
        Kilometraje = c.Kilometraje,
        NivelCombustible = c.NivelCombustible,
        Limpio = c.Limpio,
        CargoCombustible = c.CargoCombustible,
        CargoLimpieza = c.CargoLimpieza,
        CargoKmExtra = c.CargoKmExtra,
        Observaciones = c.Observaciones
    };
}
