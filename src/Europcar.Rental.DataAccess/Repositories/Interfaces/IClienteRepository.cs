using Europcar.Rental.DataAccess.Common;
using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Repositories.Interfaces;

public interface IClienteRepository : IGenericRepository<ClienteEntity>
{
    Task<ClienteEntity?> GetByIdentificacionAsync(string numeroIdentificacion);
    Task<ClienteEntity?> GetByCorreoAsync(string correo);
    Task<PagedResult<ClienteEntity>> GetPagedAsync(PaginationParams paginationParams, string? estadoCliente = null);
}
