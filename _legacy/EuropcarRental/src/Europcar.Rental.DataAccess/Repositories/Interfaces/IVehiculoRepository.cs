using Europcar.Rental.DataAccess.Common;
using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Repositories.Interfaces;

public interface IVehiculoRepository : IGenericRepository<VehiculoEntity>
{
    Task<PagedResult<VehiculoEntity>> GetDisponiblesPagedAsync(
        PaginationParams paginationParams,
        int? localizacionId = null,
        int? categoriaId = null,
        int? marcaId = null,
        string? tipoTransmision = null);
}
