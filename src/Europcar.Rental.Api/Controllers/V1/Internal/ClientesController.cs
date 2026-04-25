using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.DTOs.Request.Clientes;
using Europcar.Rental.Business.Interfaces;

namespace Europcar.Rental.Api.Controllers.V1.Internal;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>
    /// Obtener todos los clientes activos.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _clienteService.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener un cliente por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _clienteService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Crear un nuevo cliente.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> Create([FromBody] CrearClienteRequest request)
    {
        var result = await _clienteService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.IdCliente }, ApiResponse<object>.Ok(result, "Cliente creado exitosamente"));
    }

    /// <summary>
    /// Actualizar un cliente existente.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN,AGENTE_POS")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarClienteRequest request)
    {
        var result = await _clienteService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.Ok(result, "Cliente actualizado exitosamente"));
    }

    /// <summary>
    /// Eliminar un cliente (soft-delete).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "API";
        await _clienteService.DeleteAsync(id, usuario);
        return Ok(ApiResponse<object>.Ok(new { Id = id }, "Cliente eliminado exitosamente"));
    }
}
