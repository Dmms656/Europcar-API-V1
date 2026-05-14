using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IVehiculoDataService
{
    Task<IEnumerable<VehiculoModel>> GetDisponiblesAsync(int? localizacionId, int? categoriaId);
    Task<VehiculoModel?> GetByIdAsync(int id);
    Task<VehiculoModel?> GetByCodigoInternoAsync(string codigoInterno);
    Task<VehiculoModel?> GetByPlacaAsync(string placa);
    Task<IEnumerable<VehiculoModel>> GetAllAsync();
    Task<VehiculoModel> CreateAsync(VehiculoModel model);
    Task UpdateAsync(VehiculoModel model);
    Task SoftDeleteAsync(int id, string usuario);
    Task UpdateEstadoOperativoAsync(int id, string estado, string usuario);
}
