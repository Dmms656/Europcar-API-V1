using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

public interface IUsuarioDataService
{
    Task<IEnumerable<UsuarioModel>> GetAllAsync();
    Task<UsuarioModel?> GetByIdAsync(int id);
    Task<UsuarioModel?> GetByUsernameAsync(string username);
    Task<IEnumerable<string>> GetRolesAsync(int idUsuario);
    Task UpdateUltimoLoginAsync(int idUsuario);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<int> CreateUserAsync(string username, string correo, string passwordHash, string passwordSalt, int? idCliente);
    Task AssignRoleAsync(int idUsuario, string roleName);
    Task UpdateEstadoAsync(int idUsuario, string estado);
    Task UpdateRolesAsync(int idUsuario, List<string> roles);
    Task DeleteAsync(int idUsuario);
}
