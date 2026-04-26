using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IFacturaDataService
{
    Task<FacturaModel> CreateAsync(FacturaModel model, string usuario);
}
