using System.Linq;
using Europcar.Rental.DataAccess.Entities.Security;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Mappers;

public static class UsuarioMapper
{
    public static UsuarioModel ToModel(this UsuarioAppEntity entity) => new()
    {
        IdUsuario = entity.IdUsuario,
        UsuarioGuid = entity.UsuarioGuid,
        Username = entity.Username,
        Correo = entity.Correo,
        PasswordHash = entity.PasswordHash,
        PasswordSalt = entity.PasswordSalt,
        EstadoUsuario = entity.EstadoUsuario,
        Activo = entity.Activo,
        IntentosFallidos = entity.IntentosFallidos,
        BloqueadoHastaUtc = entity.BloqueadoHastaUtc,
        Roles = entity.UsuariosRoles
            .Where(ur => ur.Activo && ur.EstadoUsuarioRol == "ACT")
            .Select(ur => ur.Rol.NombreRol)
            .ToList()
    };
}
