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

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(QueryTimeout);

        try
        {
            var existing = await _db.Clientes
                .AsNoTracking()
                .Where(c => c.NumeroIdentificacion == numero && !c.EsEliminado)
                .Select(c => new { c.IdCliente, c.ClienteGuid })
                .FirstOrDefaultAsync(timeoutCts.Token);

            if (existing is not null)
            {
                await _db.Clientes
                    .Where(c => c.IdCliente == existing.IdCliente)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(c => c.CliNombre1, n1)
                        .SetProperty(c => c.CliNombre2, n2)
                        .SetProperty(c => c.CliApellido1, a1)
                        .SetProperty(c => c.CliApellido2, a2)
                        .SetProperty(c => c.CliCorreo, correo)
                        .SetProperty(c => c.CliTelefono, telefono),
                        timeoutCts.Token);

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
            await _db.SaveChangesAsync(timeoutCts.Token);

            return Ok(ApiResponse<ClienteUpsertResult>.Ok(
                new ClienteUpsertResult { IdCliente = entity.IdCliente, ClienteGuid = entity.ClienteGuid, Created = true },
                traceId: HttpContext.TraceIdentifier));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return StatusCode(504, ApiResponse<ClienteUpsertResult>.Fail(504, "Timeout en upsert de cliente.", HttpContext.TraceIdentifier));
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

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(QueryTimeout);

        try
        {
            var clienteExists = await _db.Clientes
                .AsNoTracking()
                .AnyAsync(c => c.IdCliente == idCliente && !c.EsEliminado, timeoutCts.Token);

            if (!clienteExists)
            {
                return NotFound(ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Fail(404, "Cliente no encontrado.", HttpContext.TraceIdentifier));
            }

            var existingConductores = await _db.Conductores
                .Where(c => c.IdCliente == idCliente && !c.EsEliminado)
                .ToListAsync(timeoutCts.Token);

            var byKey = existingConductores.ToDictionary(c => c.NumeroIdentificacion, StringComparer.Ordinal);
            var changed = false;

            foreach (var req in conductores)
            {
                var tipoDb = ClientesApiMapper.ToDbTipoIdentificacionConductor(req.TipoIdentificacion);
                var numero = (req.NumeroIdentificacion ?? string.Empty).Trim();
                var (n1, n2) = ClientesApiMapper.SplitTwo(req.Nombres);
                var (a1, a2) = ClientesApiMapper.SplitTwo(req.Apellidos);
                var edad = (short)Math.Clamp(req.EdadConductor, 21, 120);

                if (byKey.TryGetValue(numero, out var existing))
                {
                    existing.ConNombre1 = n1;
                    existing.ConNombre2 = n2;
                    existing.ConApellido1 = a1;
                    existing.ConApellido2 = a2;
                    existing.FechaVencimientoLicencia = req.FechaVencimientoLicencia;
                    existing.EdadConductor = edad;
                    existing.ConCorreo = req.Correo.Trim();
                    existing.ConTelefono = req.Telefono.Trim();
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
                byKey[numero] = entity;
                changed = true;
            }

            if (changed)
            {
                await _db.SaveChangesAsync(timeoutCts.Token);
            }

            var results = new List<ConductorUpsertResult>(conductores.Count);
            foreach (var req in conductores)
            {
                var numero = (req.NumeroIdentificacion ?? string.Empty).Trim();
                var saved = byKey[numero];

                results.Add(new ConductorUpsertResult
                {
                    IdConductor = saved.IdConductor,
                    ConductorGuid = saved.ConductorGuid,
                    NumeroIdentificacion = saved.NumeroIdentificacion,
                    EsPrincipal = req.EsPrincipal
                });
            }

            return Ok(ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Ok(results, traceId: HttpContext.TraceIdentifier));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return StatusCode(504, ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Fail(
                504,
                "Timeout en upsert de conductores.",
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
