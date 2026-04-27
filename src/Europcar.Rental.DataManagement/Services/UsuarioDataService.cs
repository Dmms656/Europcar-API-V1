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
            IdCliente = u.IdCliente,
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

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _context.UsuariosApp.AnyAsync(u => u.Username == username);
    }

    public async Task<int> CreateUserAsync(string username, string correo, string passwordHash, string passwordSalt, int? idCliente)
    {
        var entity = new Europcar.Rental.DataAccess.Entities.Security.UsuarioAppEntity
        {
            UsuarioGuid = Guid.NewGuid(),
            Username = username,
            Correo = correo,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            EstadoUsuario = "ACT",
            Activo = true,
            RequiereCambioPassword = false,
            IdCliente = idCliente,
            CreadoPorUsuario = "REGISTRO_WEB",
            ModificadoPorUsuario = "REGISTRO_WEB"
        };

        _context.UsuariosApp.Add(entity);
        await _context.SaveChangesAsync();
        return entity.IdUsuario;
    }

    public async Task AssignRoleAsync(int idUsuario, string roleName)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == roleName);
        if (role == null) return;

        var userRole = new Europcar.Rental.DataAccess.Entities.Security.UsuarioRolEntity
        {
            IdUsuario = idUsuario,
            IdRol = role.IdRol,
            EstadoUsuarioRol = "ACT",
            Activo = true,
            CreadoPorUsuario = "REGISTRO_WEB",
            ModificadoPorUsuario = "REGISTRO_WEB"
        };

        _context.UsuariosRoles.Add(userRole);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UsuarioModel>> GetAllAsync()
    {
        var users = await _context.UsuariosApp
            .Include(u => u.UsuariosRoles)
                .ThenInclude(ur => ur.Rol)
            .OrderBy(u => u.IdUsuario)
            .ToListAsync();

        return users.Select(u => new UsuarioModel
        {
            IdUsuario = u.IdUsuario,
            UsuarioGuid = u.UsuarioGuid,
            Username = u.Username,
            Correo = u.Correo,
            EstadoUsuario = u.EstadoUsuario,
            Activo = u.Activo,
            IntentosFallidos = u.IntentosFallidos,
            BloqueadoHastaUtc = u.BloqueadoHastaUtc,
            IdCliente = u.IdCliente,
            Roles = u.UsuariosRoles
                .Where(ur => ur.Activo && ur.EstadoUsuarioRol == "ACT")
                .Select(ur => ur.Rol.NombreRol)
                .ToList()
        });
    }

    public async Task<UsuarioModel?> GetByIdAsync(int id)
    {
        var u = await _context.UsuariosApp
            .Include(u => u.UsuariosRoles)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.IdUsuario == id);

        if (u == null) return null;

        return new UsuarioModel
        {
            IdUsuario = u.IdUsuario,
            UsuarioGuid = u.UsuarioGuid,
            Username = u.Username,
            Correo = u.Correo,
            EstadoUsuario = u.EstadoUsuario,
            Activo = u.Activo,
            IntentosFallidos = u.IntentosFallidos,
            BloqueadoHastaUtc = u.BloqueadoHastaUtc,
            IdCliente = u.IdCliente,
            Roles = u.UsuariosRoles
                .Where(ur => ur.Activo && ur.EstadoUsuarioRol == "ACT")
                .Select(ur => ur.Rol.NombreRol)
                .ToList()
        };
    }

    public async Task UpdateEstadoAsync(int idUsuario, string estado)
    {
        var user = await _context.UsuariosApp.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        if (user != null)
        {
            user.EstadoUsuario = estado;
            user.Activo = estado == "ACT";
            user.ModificadoPorUsuario = "ADMIN_PANEL";
            user.FechaModificacionUtc = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int idUsuario)
    {
        var user = await _context.UsuariosApp
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        if (user != null)
        {
            user.EsEliminado = true;
            user.Activo = false;
            user.EstadoUsuario = "INA";
            user.ModificadoPorUsuario = "ADMIN_PANEL";
            user.FechaModificacionUtc = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}

