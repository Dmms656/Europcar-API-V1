using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IConductorDataService
{
    Task<IEnumerable<ConductorModel>> GetByClienteIdAsync(int idCliente);
    Task<ConductorModel?> GetByIdAsync(int id);
    Task<ConductorModel> CreateAsync(ConductorModel model);
    Task UpdateAsync(ConductorModel model);
    Task SoftDeleteAsync(int id);
}
