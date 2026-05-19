using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedCar.Clientes.Api.Contracts;
using RedCar.Clientes.DataAccess.Context;
using RedCar.Clientes.DataAccess.Entities;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Clientes.Api.Controllers;

[ApiController]
[Route("api/v1/clientes")]
public sealed class ClientesController : ControllerBase
{
    private readonly ClientesDbContext _db;

    public ClientesController(ClientesDbContext db) => _db = db;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ClienteDetalleDto>>> GetById(int id, CancellationToken ct)
    {
        var c = await _db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdCliente == id && !x.EsEliminado && x.EstadoCliente == "ACT", ct);

        if (c is null)
        {
            return NotFound(ApiResponse<ClienteDetalleDto>.Fail(404, "Cliente no encontrado.", HttpContext.TraceIdentifier));
        }

        var dto = new ClienteDetalleDto
        {
            IdCliente = c.IdCliente,
            Nombres = ClientesApiMapper.JoinNames(c.CliNombre1, c.CliNombre2),
            Apellidos = ClientesApiMapper.JoinNames(c.CliApellido1, c.CliApellido2),
            TipoIdentificacion = ClientesApiMapper.ToApiTipoIdentificacion(c.TipoIdentificacion),
            NumeroIdentificacion = c.NumeroIdentificacion,
            Correo = c.CliCorreo,
            Telefono = c.CliTelefono
        };

        return Ok(ApiResponse<ClienteDetalleDto>.Ok(dto, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPost("upsert")]
    public async Task<ActionResult<ApiResponse<ClienteUpsertResult>>> Upsert([FromBody] ClienteUpsertRequest req, CancellationToken ct)
    {
        var tipoDb = ClientesApiMapper.ToDbTipoIdentificacion(req.TipoIdentificacion);
        var numero = (req.NumeroIdentificacion ?? string.Empty).Trim();

        var existing = await _db.Clientes.FirstOrDefaultAsync(
            c => c.TipoIdentificacion == tipoDb && c.NumeroIdentificacion == numero && !c.EsEliminado,
            ct);

        if (existing is not null)
        {
            var (n1, n2) = ClientesApiMapper.SplitTwo(req.Nombres);
            var (a1, a2) = ClientesApiMapper.SplitTwo(req.Apellidos);
            existing.CliNombre1 = n1;
            existing.CliNombre2 = n2;
            existing.CliApellido1 = a1;
            existing.CliApellido2 = a2;
            existing.CliCorreo = req.Correo.Trim();
            existing.CliTelefono = req.Telefono.Trim();
            await _db.SaveChangesAsync(ct);

            return Ok(ApiResponse<ClienteUpsertResult>.Ok(
                new ClienteUpsertResult { IdCliente = existing.IdCliente, ClienteGuid = existing.ClienteGuid, Created = false },
                traceId: HttpContext.TraceIdentifier));
        }

        var maxId = await _db.Clientes.MaxAsync(c => (int?)c.IdCliente, ct) ?? 0;
        var codigo = $"CLI-{(maxId + 1):D4}";
        var (nn1, nn2) = ClientesApiMapper.SplitTwo(req.Nombres);
        var (aa1, aa2) = ClientesApiMapper.SplitTwo(req.Apellidos);

        var entity = new Cliente
        {
            ClienteGuid = Guid.NewGuid(),
            CodigoCliente = codigo,
            TipoIdentificacion = tipoDb,
            NumeroIdentificacion = numero,
            CliNombre1 = nn1,
            CliNombre2 = nn2,
            CliApellido1 = aa1,
            CliApellido2 = aa2,
            FechaNacimiento = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
            CliTelefono = req.Telefono.Trim(),
            CliCorreo = req.Correo.Trim(),
            DireccionPrincipal = null,
            EstadoCliente = "ACT",
            EsEliminado = false,
            FechaRegistroUtc = DateTimeOffset.UtcNow,
            CreadoPorUsuario = "BOOKING_API",
            OrigenRegistro = "API"
        };

        _db.Clientes.Add(entity);
        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponse<ClienteUpsertResult>.Ok(
            new ClienteUpsertResult { IdCliente = entity.IdCliente, ClienteGuid = entity.ClienteGuid, Created = true },
            traceId: HttpContext.TraceIdentifier));
    }

    [HttpPost("{idCliente:int}/conductores/upsert")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ConductorUpsertResult>>>> UpsertConductores(
        int idCliente,
        [FromBody] IReadOnlyList<ConductorUpsertRequest> conductores,
        CancellationToken ct)
    {
        if (conductores is null || conductores.Count == 0)
        {
            return Ok(ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Ok(Array.Empty<ConductorUpsertResult>(), traceId: HttpContext.TraceIdentifier));
        }

        var clienteExists = await _db.Clientes.AnyAsync(c => c.IdCliente == idCliente && !c.EsEliminado, ct);
        if (!clienteExists)
        {
            return NotFound(ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Fail(404, "Cliente no encontrado.", HttpContext.TraceIdentifier));
        }

        var results = new List<ConductorUpsertResult>();

        foreach (var req in conductores)
        {
            var tipoDb = ClientesApiMapper.ToDbTipoIdentificacionConductor(req.TipoIdentificacion);
            var numero = (req.NumeroIdentificacion ?? string.Empty).Trim();

            var existing = await _db.Conductores.FirstOrDefaultAsync(
                c => c.IdCliente == idCliente && c.TipoIdentificacion == tipoDb && c.NumeroIdentificacion == numero && !c.EsEliminado,
                ct);

            if (existing is not null)
            {
                var (n1, n2) = ClientesApiMapper.SplitTwo(req.Nombres);
                var (a1, a2) = ClientesApiMapper.SplitTwo(req.Apellidos);
                existing.ConNombre1 = n1;
                existing.ConNombre2 = n2;
                existing.ConApellido1 = a1;
                existing.ConApellido2 = a2;
                existing.FechaVencimientoLicencia = req.FechaVencimientoLicencia;
                existing.EdadConductor = (short)Math.Clamp(req.EdadConductor, 21, 120);
                existing.ConCorreo = req.Correo.Trim();
                existing.ConTelefono = req.Telefono.Trim();
                await _db.SaveChangesAsync(ct);

                results.Add(new ConductorUpsertResult
                {
                    IdConductor = existing.IdConductor,
                    ConductorGuid = existing.ConductorGuid,
                    NumeroIdentificacion = existing.NumeroIdentificacion,
                    EsPrincipal = req.EsPrincipal
                });
                continue;
            }

            var licencia = $"LIC-{Guid.NewGuid():N}"[..30];
            var (nn1, nn2) = ClientesApiMapper.SplitTwo(req.Nombres);
            var (aa1, aa2) = ClientesApiMapper.SplitTwo(req.Apellidos);

            var entity = new Conductor
            {
                ConductorGuid = Guid.NewGuid(),
                CodigoConductor = "TEMP",
                IdCliente = idCliente,
                TipoIdentificacion = tipoDb,
                NumeroIdentificacion = numero,
                ConNombre1 = nn1,
                ConNombre2 = nn2,
                ConApellido1 = aa1,
                ConApellido2 = aa2,
                NumeroLicencia = licencia,
                FechaVencimientoLicencia = req.FechaVencimientoLicencia,
                EdadConductor = (short)Math.Clamp(req.EdadConductor, 21, 120),
                ConTelefono = req.Telefono.Trim(),
                ConCorreo = req.Correo.Trim(),
                EstadoConductor = "ACT",
                EsEliminado = false,
                FechaRegistroUtc = DateTimeOffset.UtcNow,
                CreadoPorUsuario = "BOOKING_API",
                OrigenRegistro = "API"
            };

            _db.Conductores.Add(entity);
            await _db.SaveChangesAsync(ct);
            entity.CodigoConductor = $"CON-{entity.IdConductor:D5}";
            await _db.SaveChangesAsync(ct);

            results.Add(new ConductorUpsertResult
            {
                IdConductor = entity.IdConductor,
                ConductorGuid = entity.ConductorGuid,
                NumeroIdentificacion = entity.NumeroIdentificacion,
                EsPrincipal = req.EsPrincipal
            });
        }

        return Ok(ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Ok(results, traceId: HttpContext.TraceIdentifier));
    }
}
