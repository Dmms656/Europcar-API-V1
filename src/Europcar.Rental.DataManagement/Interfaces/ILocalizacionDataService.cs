using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface ILocalizacionDataService
{
    Task<IEnumerable<LocalizacionModel>> GetAllAsync(bool soloActivas = true);
    Task<LocalizacionModel?> GetByIdAsync(int id);
    Task<LocalizacionModel?> GetByCodigoAsync(string codigo);
    Task<LocalizacionModel> CreateAsync(LocalizacionModel model, string usuario);
    Task UpdateAsync(LocalizacionModel model, string usuario);
    Task UpdateEstadoAsync(int id, string estado, string usuario, string? motivo = null);
    Task SoftDeleteAsync(int id, string usuario, string? motivo = null);
}
