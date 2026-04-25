using Europcar.Rental.DataAccess.Common;
using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Repositories.Interfaces;

public interface IReservaRepository : IGenericRepository<ReservaEntity>
{
    Task<ReservaEntity?> GetByCodigoAsync(string codigoReserva);
    Task<bool> ExisteSolapamientoAsync(int idVehiculo, DateTimeOffset fechaInicio, DateTimeOffset fechaFin, int? idReservaExcluir = null);
    Task<PagedResult<ReservaEntity>> GetPagedByClienteAsync(int idCliente, PaginationParams paginationParams);
}
