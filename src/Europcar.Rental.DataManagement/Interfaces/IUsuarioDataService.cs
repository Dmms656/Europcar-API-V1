using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IUsuarioDataService
{
    Task<UsuarioModel?> GetByUsernameAsync(string username);
    Task<IEnumerable<string>> GetRolesAsync(int idUsuario);
    Task UpdateUltimoLoginAsync(int idUsuario);
}
