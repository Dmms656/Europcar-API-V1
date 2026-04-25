using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IClienteDataService
{
    Task<IEnumerable<ClienteModel>> GetAllAsync();
    Task<ClienteModel?> GetByIdAsync(int id);
    Task<ClienteModel?> GetByIdentificacionAsync(string numeroIdentificacion);
    Task<ClienteModel> CreateAsync(ClienteModel model);
    Task UpdateAsync(ClienteModel model);
    Task SoftDeleteAsync(int id, string usuario);
}
