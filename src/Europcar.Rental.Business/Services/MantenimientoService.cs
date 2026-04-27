using Europcar.Rental.Business.DTOs.Request.Mantenimientos;
using Europcar.Rental.Business.DTOs.Response.Mantenimientos;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Common;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.Business.Services;

public class MantenimientoService : IMantenimientoService
{
    private readonly IMantenimientoDataService _mantenimientoDataService;
    private readonly IVehiculoDataService _vehiculoDataService;
    private readonly IUnitOfWork _unitOfWork;

    public MantenimientoService(
        IMantenimientoDataService mantenimientoDataService,
        IVehiculoDataService vehiculoDataService,
        IUnitOfWork unitOfWork)
    {
        _mantenimientoDataService = mantenimientoDataService;
        _vehiculoDataService = vehiculoDataService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MantenimientoResponse>> GetAllAsync()
    {
        var lista = await _mantenimientoDataService.GetAllAsync();
        return lista.Select(MapToResponse);
    }

    public async Task<MantenimientoResponse> GetByIdAsync(int id)
    {
        var m = await _mantenimientoDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Mantenimiento con ID {id} no encontrado");
        return MapToResponse(m);
    }

    public async Task<IEnumerable<MantenimientoResponse>> GetByVehiculoIdAsync(int idVehiculo)
    {
        var lista = await _mantenimientoDataService.GetByVehiculoIdAsync(idVehiculo);
        return lista.Select(MapToResponse);
    }

    public async Task<MantenimientoResponse> CreateAsync(CrearMantenimientoRequest request, string usuario)
    {
        var vehiculo = await _vehiculoDataService.GetByIdAsync(request.IdVehiculo)
            ?? throw new NotFoundException($"Vehículo con ID {request.IdVehiculo} no encontrado");

        if (vehiculo.EstadoOperativo == "ALQUILADO")
            throw new BusinessException("No se puede enviar a mantenimiento un vehículo que está alquilado");

        var codigo = $"MNT-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var model = new MantenimientoModel
        {
            CodigoMantenimiento = codigo,
            IdVehiculo = request.IdVehiculo,
            TipoMantenimiento = request.TipoMantenimiento,
            FechaInicioUtc = DateTimeOffset.UtcNow,
            KilometrajeMantenimiento = request.KilometrajeMantenimiento,
            CostoMantenimiento = request.CostoMantenimiento,
            ProveedorTaller = request.ProveedorTaller,
            Observaciones = request.Observaciones
        };

        // El trigger fn_post_mantenimiento_estado_vehiculo cambia el estado del vehículo
        var created = await _mantenimientoDataService.CreateAsync(model, usuario);
        await _unitOfWork.SaveChangesAsync();

        var result = await _mantenimientoDataService.GetByIdAsync(created.IdMantenimiento);
        return MapToResponse(result!);
    }

    public async Task<MantenimientoResponse> CerrarAsync(int id, string usuario)
    {
        var m = await _mantenimientoDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Mantenimiento con ID {id} no encontrado");

        if (m.EstadoMantenimiento != "ABIERTO")
            throw new BusinessException("Solo se puede cerrar un mantenimiento abierto");

        // El trigger devuelve el vehículo a DISPONIBLE
        await _mantenimientoDataService.CerrarAsync(id, usuario);
        await _unitOfWork.SaveChangesAsync();

        var result = await _mantenimientoDataService.GetByIdAsync(id);
        return MapToResponse(result!);
    }

    private static MantenimientoResponse MapToResponse(MantenimientoModel m) => new()
    {
        IdMantenimiento = m.IdMantenimiento,
        MantenimientoGuid = m.MantenimientoGuid,
        CodigoMantenimiento = m.CodigoMantenimiento,
        TipoMantenimiento = m.TipoMantenimiento,
        FechaInicioUtc = m.FechaInicioUtc,
        FechaFinUtc = m.FechaFinUtc,
        KilometrajeMantenimiento = m.KilometrajeMantenimiento,
        CostoMantenimiento = m.CostoMantenimiento,
        ProveedorTaller = m.ProveedorTaller,
        EstadoMantenimiento = m.EstadoMantenimiento,
        Observaciones = m.Observaciones,
        PlacaVehiculo = m.PlacaVehiculo
    };
}
