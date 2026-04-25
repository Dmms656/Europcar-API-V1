using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IPagoDataService
{
    Task<PagoModel?> GetByIdAsync(int id);
    Task<IEnumerable<PagoModel>> GetByReservaIdAsync(int idReserva);
    Task<PagoModel> CreateAsync(PagoModel model, string usuario);
    Task UpdateEstadoAsync(int id, string estado, string usuario);
}
