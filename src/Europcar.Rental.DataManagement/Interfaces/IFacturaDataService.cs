using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IFacturaDataService
{
    Task<FacturaModel> AddAsync(FacturaModel model, string usuario);
    Task<FacturaModel> CreateAsync(FacturaModel model, string usuario);
}
