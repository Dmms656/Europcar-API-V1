using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface ICatalogoDataService
{
    Task<IEnumerable<CatalogoModel>> GetCategoriasAsync();
    Task<IEnumerable<CatalogoModel>> GetMarcasAsync();
    Task<IEnumerable<CatalogoModel>> GetExtrasAsync();
}
