using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class UsuarioDataService : IUsuarioDataService
{
    private readonly RentalDbContext _context;

    public UsuarioDataService(RentalDbContext context) => _context = context;

    public async Task<UsuarioModel?> GetByUsernameAsync(string username)
    {
        var u = await _context.UsuariosApp
            .Include(u => u.UsuariosRoles)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (u == null) return null;

        return new UsuarioModel
        {
            IdUsuario = u.IdUsuario,
            UsuarioGuid = u.UsuarioGuid,
            Username = u.Username,
            Correo = u.Correo,
            PasswordHash = u.PasswordHash,
            PasswordSalt = u.PasswordSalt,
            EstadoUsuario = u.EstadoUsuario,
            Activo = u.Activo,
            IntentosFallidos = u.IntentosFallidos,
            BloqueadoHastaUtc = u.BloqueadoHastaUtc,
            Roles = u.UsuariosRoles
                .Where(ur => ur.Activo && ur.EstadoUsuarioRol == "ACT")
                .Select(ur => ur.Rol.NombreRol)
                .ToList()
        };
    }

    public async Task<IEnumerable<string>> GetRolesAsync(int idUsuario)
    {
        return await _context.UsuariosRoles
            .Include(ur => ur.Rol)
            .Where(ur => ur.IdUsuario == idUsuario && ur.Activo && ur.EstadoUsuarioRol == "ACT")
            .Select(ur => ur.Rol.NombreRol)
            .ToListAsync();
    }

    public async Task UpdateUltimoLoginAsync(int idUsuario)
    {
        var user = await _context.UsuariosApp
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (user != null)
        {
            user.UltimoLoginUtc = DateTimeOffset.UtcNow;
            user.IntentosFallidos = 0;
            user.ModificadoPorUsuario = "LOGIN_OK";
        }
    }
}
