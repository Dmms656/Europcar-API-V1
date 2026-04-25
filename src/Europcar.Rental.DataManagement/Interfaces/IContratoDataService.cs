using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IContratoDataService
{
    Task<ContratoModel?> GetByIdAsync(int id);
    Task<ContratoModel?> GetByReservaIdAsync(int idReserva);
    Task<IEnumerable<ContratoModel>> GetAllAsync();
    Task<ContratoModel> CreateAsync(ContratoModel model, string usuario);
    Task UpdateEstadoAsync(int id, string estado, string usuario);
}
