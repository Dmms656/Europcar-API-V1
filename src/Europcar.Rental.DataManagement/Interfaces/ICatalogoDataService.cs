using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface ICatalogoDataService
{
    Task<IEnumerable<CatalogoModel>> GetCategoriasAsync();
    Task<IEnumerable<CatalogoModel>> GetMarcasAsync();
    Task<IEnumerable<CatalogoModel>> GetExtrasAsync();
    Task<CatalogoModel?> GetExtraByIdAsync(int id);
    Task<CatalogoModel?> GetExtraByCodigoAsync(string codigo);
    Task<CatalogoModel> CreateExtraAsync(CatalogoModel model, string usuario);
    Task UpdateExtraAsync(CatalogoModel model, string usuario);
    Task UpdateExtraEstadoAsync(int id, string estado, string usuario, string? motivo = null);
    Task SoftDeleteExtraAsync(int id, string usuario, string? motivo = null);
}
