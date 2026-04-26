using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IReservaDataService
{
    Task<ReservaModel?> GetByCodigoAsync(string codigo);
    Task<ReservaModel?> GetByIdAsync(int id);
    Task<IEnumerable<ReservaModel>> GetByClienteIdAsync(int idCliente);
    Task<ReservaModel> CreateAsync(ReservaModel model);
    Task<bool> ExisteSolapamientoAsync(int idVehiculo, DateTimeOffset fechaInicio, DateTimeOffset fechaFin);
    Task UpdateEstadoAsync(int id, string estado, string usuario, string? motivo = null);
    Task AddConductorAsync(int idReserva, int idConductor, bool esPrincipal, decimal cargoConductorJoven);
}
