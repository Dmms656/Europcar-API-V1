using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IMantenimientoDataService
{
    Task<IEnumerable<MantenimientoModel>> GetAllAsync();
    Task<MantenimientoModel?> GetByIdAsync(int id);
    Task<IEnumerable<MantenimientoModel>> GetByVehiculoIdAsync(int idVehiculo);
    Task<MantenimientoModel> CreateAsync(MantenimientoModel model, string usuario);
    Task CerrarAsync(int id, string usuario);
}
