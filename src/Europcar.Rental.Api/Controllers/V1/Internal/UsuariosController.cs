using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.Services;
using Europcar.Rental.DataManagement.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "ADMIN")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioDataService _usuarioDataService;

    public UsuariosController(IUsuarioDataService usuarioDataService)
    {
        _usuarioDataService = usuarioDataService;
    }

    /// <summary>
    /// Obtener todos los usuarios del sistema.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _usuarioDataService.GetAllAsync();
        var result = usuarios.Select(u => new
        {
            u.IdUsuario,
            u.Username,
            u.Correo,
            u.EstadoUsuario,
            u.Activo,
            u.Roles,
            u.IdCliente
        });
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener un usuario por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _usuarioDataService.GetByIdAsync(id);
        if (usuario == null) return NotFound(ApiResponse<object>.Fail("Usuario no encontrado"));
        return Ok(ApiResponse<object>.Ok(new
        {
            usuario.IdUsuario,
            usuario.Username,
            usuario.Correo,
            usuario.EstadoUsuario,
            usuario.Activo,
            usuario.Roles,
            usuario.IdCliente
        }));
    }

    /// <summary>
    /// Crear un nuevo usuario con roles asignados.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest request)
    {
        if (await _usuarioDataService.ExistsByUsernameAsync(request.Username))
            return Conflict(ApiResponse<object>.Fail("El nombre de usuario ya existe"));

        var (hash, salt) = AuthService.CreatePasswordHash(request.Password);
        var userId = await _usuarioDataService.CreateUserAsync(
            request.Username, request.Correo, hash, salt, null);

        foreach (var role in request.Roles)
        {
            await _usuarioDataService.AssignRoleAsync(userId, role);
        }

        return Ok(ApiResponse<object>.Ok(new { userId, request.Username }, "Usuario creado exitosamente"));
    }

    /// <summary>
    /// Activar o desactivar un usuario.
    /// </summary>
    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> UpdateEstado(int id, [FromBody] UpdateEstadoRequest request)
    {
        await _usuarioDataService.UpdateEstadoAsync(id, request.Estado);
        return Ok(ApiResponse<object>.Ok(null, "Estado actualizado"));
    }

    /// <summary>
    /// Eliminar (soft delete) un usuario.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _usuarioDataService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(null, "Usuario eliminado"));
    }
}

public class CreateUsuarioRequest
{
    public string Username { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public class UpdateEstadoRequest
{
    public string Estado { get; set; } = "ACT";
}
