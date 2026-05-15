using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedCar.Clientes.Api.Contracts;
using RedCar.Clientes.DataAccess.Context;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Clientes.Api.Controllers;

[ApiController]
[Route("api/v1/conductores")]
public sealed class ConductoresController : ControllerBase
{
    private readonly ClientesDbContext _db;

    public ConductoresController(ClientesDbContext db) => _db = db;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ConductorDetalleDto>>> GetById(int id, CancellationToken ct)
    {
        var c = await _db.Conductores.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdConductor == id && !x.EsEliminado && x.EstadoConductor == "ACT", ct);

        if (c is null)
        {
            return NotFound(ApiResponse<ConductorDetalleDto>.Fail(404, "Conductor no encontrado.", HttpContext.TraceIdentifier));
        }

        var dto = new ConductorDetalleDto
        {
            IdConductor = c.IdConductor,
            Nombres = ClientesApiMapper.JoinNames(c.ConNombre1, c.ConNombre2),
            Apellidos = ClientesApiMapper.JoinNames(c.ConApellido1, c.ConApellido2),
            TipoIdentificacion = ClientesApiMapper.ToApiTipoIdentificacion(c.TipoIdentificacion),
            NumeroIdentificacion = c.NumeroIdentificacion,
            EdadConductor = c.EdadConductor
        };

        return Ok(ApiResponse<ConductorDetalleDto>.Ok(dto, traceId: HttpContext.TraceIdentifier));
    }
}
