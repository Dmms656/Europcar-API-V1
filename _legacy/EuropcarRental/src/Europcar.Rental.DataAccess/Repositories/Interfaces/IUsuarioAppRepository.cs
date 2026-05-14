using Europcar.Rental.DataAccess.Common;
using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Repositories.Interfaces;

public interface IUsuarioAppRepository : IGenericRepository<Entities.Security.UsuarioAppEntity>
{
    Task<Entities.Security.UsuarioAppEntity?> GetByUsernameWithRolesAsync(string username);
}
