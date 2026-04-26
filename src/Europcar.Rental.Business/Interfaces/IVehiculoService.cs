using Europcar.Rental.Business.DTOs.Request.Vehiculos;
using Europcar.Rental.Business.DTOs.Response.Vehiculos;

namespace Europcar.Rental.Business.Interfaces;

public interface IVehiculoService
{
    Task<IEnumerable<VehiculoResponse>> GetAllAsync();
    Task<IEnumerable<VehiculoDisponibleResponse>> GetDisponiblesAsync(BuscarVehiculosRequest request);
    Task<VehiculoResponse> GetByIdAsync(int id);
    Task<VehiculoResponse> CreateAsync(CrearVehiculoRequest request);
    Task<VehiculoResponse> UpdateAsync(int id, ActualizarVehiculoRequest request);
    Task DeleteAsync(int id, string usuario);
}
