using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface ICatalogoDataService
{
    Task<IEnumerable<CatalogoModel>> GetPaisesAsync();
    Task<CatalogoModel?> GetPaisByIdAsync(int id);
    Task<CatalogoModel?> GetPaisByCodigoIso2Async(string codigoIso2);
    Task<CatalogoModel> CreatePaisAsync(CatalogoModel model, string usuario);
    Task UpdatePaisAsync(CatalogoModel model, string usuario);
    Task UpdatePaisEstadoAsync(int id, string estado, string usuario, string? motivo = null);
    Task SoftDeletePaisAsync(int id, string usuario, string? motivo = null);

    Task<IEnumerable<CiudadModel>> GetCiudadesAsync();
    Task<CiudadModel?> GetCiudadByIdAsync(int id);
    Task<CiudadModel> CreateCiudadAsync(CiudadModel model, string usuario);
    Task UpdateCiudadAsync(CiudadModel model, string usuario);
    Task UpdateCiudadEstadoAsync(int id, string estado, string usuario, string? motivo = null);
    Task SoftDeleteCiudadAsync(int id, string usuario, string? motivo = null);

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
