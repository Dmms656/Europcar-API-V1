using Europcar.Rental.Business.DTOs.Request.Vehiculos;
using Europcar.Rental.Business.DTOs.Response.Vehiculos;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Interfaces;

namespace Europcar.Rental.Business.Services;

public class VehiculoService : IVehiculoService
{
    private readonly IVehiculoDataService _vehiculoDataService;

    public VehiculoService(IVehiculoDataService vehiculoDataService)
    {
        _vehiculoDataService = vehiculoDataService;
    }

    public async Task<IEnumerable<VehiculoDisponibleResponse>> GetDisponiblesAsync(BuscarVehiculosRequest request)
    {
        var vehiculos = await _vehiculoDataService.GetDisponiblesAsync(request.LocalizacionId, request.CategoriaId);
        return vehiculos.Select(MapToResponse);
    }

    public async Task<VehiculoDisponibleResponse> GetByIdAsync(int id)
    {
        var vehiculo = await _vehiculoDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Vehículo con ID {id} no encontrado");
        return MapToResponse(vehiculo);
    }

    private static VehiculoDisponibleResponse MapToResponse(DataManagement.Models.VehiculoModel v) => new()
    {
        IdVehiculo = v.IdVehiculo,
        VehiculoGuid = v.VehiculoGuid,
        CodigoInterno = v.CodigoInterno,
        Placa = v.Placa,
        Marca = v.Marca,
        Categoria = v.Categoria,
        Modelo = v.Modelo,
        AnioFabricacion = v.AnioFabricacion,
        Color = v.Color,
        TipoCombustible = v.TipoCombustible,
        TipoTransmision = v.TipoTransmision,
        CapacidadPasajeros = v.CapacidadPasajeros,
        CapacidadMaletas = v.CapacidadMaletas,
        PrecioBaseDia = v.PrecioBaseDia,
        AireAcondicionado = v.AireAcondicionado,
        ImagenUrl = v.ImagenUrl,
        IdLocalizacion = v.IdLocalizacion,
        Localizacion = v.NombreLocalizacion
    };
}
