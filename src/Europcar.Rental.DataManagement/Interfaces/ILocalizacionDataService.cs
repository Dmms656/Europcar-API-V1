using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface ILocalizacionDataService
{
    Task<IEnumerable<LocalizacionModel>> GetAllAsync();
    Task<LocalizacionModel?> GetByIdAsync(int id);
}
