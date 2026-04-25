using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IVehiculoDataService
{
    Task<IEnumerable<VehiculoModel>> GetDisponiblesAsync(int? localizacionId, int? categoriaId);
    Task<VehiculoModel?> GetByIdAsync(int id);
    Task<IEnumerable<VehiculoModel>> GetAllAsync();
    Task UpdateEstadoOperativoAsync(int id, string estado, string usuario);
}
