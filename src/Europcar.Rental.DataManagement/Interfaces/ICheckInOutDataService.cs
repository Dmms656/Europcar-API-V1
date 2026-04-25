using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface ICheckInOutDataService
{
    Task<CheckInOutModel> CreateAsync(CheckInOutModel model, string usuario);
    Task<IEnumerable<CheckInOutModel>> GetByContratoIdAsync(int idContrato);
}
