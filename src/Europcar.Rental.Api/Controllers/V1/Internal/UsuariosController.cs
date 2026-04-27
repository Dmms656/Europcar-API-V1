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
    private readonly IClienteDataService _clienteDataService;

    public UsuariosController(IUsuarioDataService usuarioDataService, IClienteDataService clienteDataService)
    {
        _usuarioDataService = usuarioDataService;
        _clienteDataService = clienteDataService;
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
        if (await _usuarioDataService.ExistsByCorreoAsync(request.Correo))
            return Conflict(ApiResponse<object>.Fail("El correo ya existe"));

        var nombre = string.IsNullOrWhiteSpace(request.Nombre) ? request.Username.Trim() : request.Nombre.Trim();
        var apellido = string.IsNullOrWhiteSpace(request.Apellido) ? "INTERNO" : request.Apellido.Trim();
        var cliente = await _clienteDataService.CreateAsync(new DataManagement.Models.ClienteModel
        {
            CodigoCliente = GenerateClientCode(),
            TipoIdentificacion = "CED",
            NumeroIdentificacion = BuildClientIdentification(request.Cedula),
            Nombre1 = nombre,
            Apellido1 = apellido,
            Telefono = string.IsNullOrWhiteSpace(request.Telefono) ? "0000000000" : request.Telefono.Trim(),
            Correo = request.Correo,
            DireccionPrincipal = request.Direccion,
            FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-25))
        });

        var (hash, salt) = AuthService.CreatePasswordHash(request.Password);
        var userId = await _usuarioDataService.CreateUserAsync(
            request.Username, request.Correo, hash, salt, cliente.IdCliente);

        var roles = request.Roles
            .Append("CLIENTE_WEB")
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
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

    /// <summary>
    /// Actualizar los roles de un usuario.
    /// </summary>
    [HttpPut("{id:int}/roles")]
    public async Task<IActionResult> UpdateRoles(int id, [FromBody] UpdateRolesRequest request)
    {
        await _usuarioDataService.UpdateRolesAsync(id, request.Roles);
        return Ok(ApiResponse<object>.Ok(null, "Roles actualizados"));
    }

    private static string GenerateClientCode()
    {
        return $"CLT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }

    private static string BuildClientIdentification(string? cedula)
    {
        if (!string.IsNullOrWhiteSpace(cedula))
            return cedula.Trim();

        return $"AUTO-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
    }
}

public class CreateUsuarioRequest
{
    public string Username { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}

public class UpdateEstadoRequest
{
    public string Estado { get; set; } = "ACT";
}

public class UpdateRolesRequest
{
    public List<string> Roles { get; set; } = new();
}
