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
    private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(20);
    private readonly ClientesDbContext _db;

    public ClientesController(ClientesDbContext db) => _db = db;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ClienteDetalleDto>>> GetById(int id, CancellationToken ct)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(QueryTimeout);

        try
        {
            var c = await _db.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdCliente == id && !x.EsEliminado && x.EstadoCliente == "ACT", timeoutCts.Token);

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
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return StatusCode(504, ApiResponse<ClienteDetalleDto>.Fail(504, "Timeout consultando cliente.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("upsert")]
    public async Task<ActionResult<ApiResponse<ClienteUpsertResult>>> Upsert([FromBody] ClienteUpsertRequest req, CancellationToken ct)
    {
        var tipoDb = ClientesApiMapper.ToDbTipoIdentificacion(req.TipoIdentificacion);
        var numero = (req.NumeroIdentificacion ?? string.Empty).Trim();
        var (n1, n2) = ClientesApiMapper.SplitTwo(req.Nombres);
        var (a1, a2) = ClientesApiMapper.SplitTwo(req.Apellidos);
        var correo = req.Correo.Trim();
        var telefono = req.Telefono.Trim();

        try
        {
            var existing = await _db.Clientes
                .FirstOrDefaultAsync(c => c.NumeroIdentificacion == numero && !c.EsEliminado, ct);

            if (existing is not null)
            {
                existing.CliNombre1 = n1;
                existing.CliNombre2 = n2;
                existing.CliApellido1 = a1;
                existing.CliApellido2 = a2;
                existing.CliCorreo = correo;
                existing.CliTelefono = telefono;
                await _db.SaveChangesAsync(ct);

                return Ok(ApiResponse<ClienteUpsertResult>.Ok(
                    new ClienteUpsertResult { IdCliente = existing.IdCliente, ClienteGuid = existing.ClienteGuid, Created = false },
                    traceId: HttpContext.TraceIdentifier));
            }

            var entity = new Cliente
            {
                ClienteGuid = Guid.NewGuid(),
                CodigoCliente = BuildCodigoCliente(tipoDb, numero),
                TipoIdentificacion = tipoDb,
                NumeroIdentificacion = numero,
                CliNombre1 = n1,
                CliNombre2 = n2,
                CliApellido1 = a1,
                CliApellido2 = a2,
                FechaNacimiento = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                CliTelefono = telefono,
                CliCorreo = correo,
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
        catch (DbUpdateException ex)
        {
            return StatusCode(500, ApiResponse<ClienteUpsertResult>.Fail(500, ex.InnerException?.Message ?? ex.Message, HttpContext.TraceIdentifier));
        }
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

        try
        {
            var clienteExists = await _db.Clientes
                .AsNoTracking()
                .AnyAsync(c => c.IdCliente == idCliente && !c.EsEliminado, ct);

            if (!clienteExists)
            {
                return NotFound(ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Fail(404, "Cliente no encontrado.", HttpContext.TraceIdentifier));
            }

            var saved = new Dictionary<string, Conductor>(conductores.Count, StringComparer.Ordinal);
            var changed = false;

            foreach (var req in conductores)
            {
                var tipoDb = ClientesApiMapper.ToDbTipoIdentificacionConductor(req.TipoIdentificacion);
                var numero = (req.NumeroIdentificacion ?? string.Empty).Trim();
                var (n1, n2) = ClientesApiMapper.SplitTwo(req.Nombres);
                var (a1, a2) = ClientesApiMapper.SplitTwo(req.Apellidos);
                var edad = (short)Math.Clamp(req.EdadConductor, 21, 120);

                var existing = await _db.Conductores
                    .FirstOrDefaultAsync(c => c.IdCliente == idCliente && c.NumeroIdentificacion == numero && !c.EsEliminado, ct);

                if (existing is not null)
                {
                    if (existing.IdCliente != idCliente)
                    {
                        return BadRequest(ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Fail(
                            400,
                            $"El conductor {numero} pertenece a otro cliente.",
                            HttpContext.TraceIdentifier));
                    }

                    existing.ConNombre1 = n1;
                    existing.ConNombre2 = n2;
                    existing.ConApellido1 = a1;
                    existing.ConApellido2 = a2;
                    existing.FechaVencimientoLicencia = req.FechaVencimientoLicencia;
                    existing.EdadConductor = edad;
                    existing.ConCorreo = req.Correo.Trim();
                    existing.ConTelefono = req.Telefono.Trim();
                    saved[numero] = existing;
                    changed = true;
                    continue;
                }

                var entity = new Conductor
                {
                    ConductorGuid = Guid.NewGuid(),
                    CodigoConductor = BuildCodigoConductor(numero),
                    IdCliente = idCliente,
                    TipoIdentificacion = tipoDb,
                    NumeroIdentificacion = numero,
                    ConNombre1 = n1,
                    ConNombre2 = n2,
                    ConApellido1 = a1,
                    ConApellido2 = a2,
                    NumeroLicencia = BuildNumeroLicencia(numero),
                    FechaVencimientoLicencia = req.FechaVencimientoLicencia,
                    EdadConductor = edad,
                    ConTelefono = req.Telefono.Trim(),
                    ConCorreo = req.Correo.Trim(),
                    EstadoConductor = "ACT",
                    EsEliminado = false,
                    FechaRegistroUtc = DateTimeOffset.UtcNow,
                    CreadoPorUsuario = "BOOKING_API",
                    OrigenRegistro = "API"
                };

                _db.Conductores.Add(entity);
                saved[numero] = entity;
                changed = true;
            }

            if (changed)
            {
                await _db.SaveChangesAsync(ct);
            }

            var results = new List<ConductorUpsertResult>(conductores.Count);
            foreach (var req in conductores)
            {
                var numero = (req.NumeroIdentificacion ?? string.Empty).Trim();
                var row = saved[numero];

                results.Add(new ConductorUpsertResult
                {
                    IdConductor = row.IdConductor,
                    ConductorGuid = row.ConductorGuid,
                    NumeroIdentificacion = row.NumeroIdentificacion,
                    EsPrincipal = req.EsPrincipal
                });
            }

            return Ok(ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Ok(results, traceId: HttpContext.TraceIdentifier));
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Fail(
                500,
                ex.InnerException?.Message ?? ex.Message,
                HttpContext.TraceIdentifier));
        }
    }

    private static string BuildCodigoCliente(string tipoDb, string numero) =>
        Truncate($"CLI-{tipoDb}-{numero}", 20);

    private static string BuildCodigoConductor(string numero) =>
        Truncate($"CON-{numero}", 20);

    private static string BuildNumeroLicencia(string numero) =>
        Truncate($"LIC-{numero}", 30);

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
