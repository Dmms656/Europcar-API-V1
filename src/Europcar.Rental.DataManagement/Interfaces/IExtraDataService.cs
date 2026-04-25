using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IExtraDataService
{
    Task<ExtraDetailModel?> GetByIdAsync(int id);
    Task<int> GetStockDisponibleAsync(int idLocalizacion, int idExtra);
    Task ReservarStockAsync(int idLocalizacion, int idExtra, int cantidad);
    Task LiberarStockAsync(int idLocalizacion, int idExtra, int cantidad);
}
