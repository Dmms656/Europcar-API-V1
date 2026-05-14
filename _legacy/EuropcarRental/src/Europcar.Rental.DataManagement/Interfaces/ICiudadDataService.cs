using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface ICiudadDataService
{
    Task<IEnumerable<CiudadModel>> GetAllAsync();
    Task<CiudadModel?> GetByIdAsync(int id);
}
