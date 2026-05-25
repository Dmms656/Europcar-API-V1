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

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClienteListItemDto>>>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 500,
        CancellationToken ct = default)
    {
        limit = limit is < 1 or > 1000 ? 500 : limit;
        page = page < 1 ? 1 : page;

        var rows = await _db.Clientes
            .AsNoTracking()
            .Where(c => !c.EsEliminado)
            .OrderBy(c => c.IdCliente)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(ct);

        var items = rows.Select(MapListItem).ToList();
        return Ok(ApiResponse<IReadOnlyList<ClienteListItemDto>>.Ok(items, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("by-identificacion/{numero}")]
    public async Task<ActionResult<ApiResponse<ClienteDetalleDto>>> GetByIdentificacion(
        [FromRoute] string numero,
        CancellationToken ct)
    {
        var doc = (numero ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(doc))
            return BadRequest(ApiResponse<ClienteDetalleDto>.Fail(400, "Número de identificación requerido.", HttpContext.TraceIdentifier));

        var c = await _db.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.NumeroIdentificacion == doc && !x.EsEliminado && x.EstadoCliente == "ACT", ct);

        if (c is null)
            return NotFound(ApiResponse<ClienteDetalleDto>.Fail(404, "Cliente no encontrado.", HttpContext.TraceIdentifier));

        return Ok(ApiResponse<ClienteDetalleDto>.Ok(MapDetalle(c), traceId: HttpContext.TraceIdentifier));
    }

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

            return Ok(ApiResponse<ClienteDetalleDto>.Ok(MapDetalle(c), traceId: HttpContext.TraceIdentifier));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return StatusCode(504, ApiResponse<ClienteDetalleDto>.Fail(504, "Timeout consultando cliente.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClienteListItemDto>>> Create(
        [FromBody] CrearClienteRequest req,
        CancellationToken ct)
    {
        var tipoDb = ClientesApiMapper.ToDbTipoIdentificacion(req.TipoIdentificacion);
        var numero = (req.NumeroIdentificacion ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(numero))
            return BadRequest(ApiResponse<ClienteListItemDto>.Fail(400, "Número de identificación requerido.", HttpContext.TraceIdentifier));

        if (await _db.Clientes.AnyAsync(c => c.NumeroIdentificacion == numero && !c.EsEliminado, ct))
            return Conflict(ApiResponse<ClienteListItemDto>.Fail(409, "Cliente ya registrado.", HttpContext.TraceIdentifier));

        var entity = new Cliente
        {
            ClienteGuid = Guid.NewGuid(),
            CodigoCliente = BuildCodigoCliente(tipoDb, numero),
            TipoIdentificacion = tipoDb,
            NumeroIdentificacion = numero,
            CliNombre1 = req.Nombre1.Trim(),
            CliNombre2 = req.Nombre2?.Trim(),
            CliApellido1 = req.Apellido1.Trim(),
            CliApellido2 = req.Apellido2?.Trim(),
            FechaNacimiento = req.FechaNacimiento == default ? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)) : req.FechaNacimiento,
            CliTelefono = req.Telefono.Trim(),
            CliCorreo = req.Correo.Trim(),
            DireccionPrincipal = req.DireccionPrincipal?.Trim(),
            EstadoCliente = "ACT",
            EsEliminado = false,
            FechaRegistroUtc = DateTimeOffset.UtcNow,
            CreadoPorUsuario = "ADMIN_API",
            OrigenRegistro = "ADMIN_API"
        };
        _db.Clientes.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<ClienteListItemDto>.Ok(MapListItem(entity), "Cliente creado exitosamente", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ClienteListItemDto>>> Update(
        int id,
        [FromBody] ActualizarClienteRequest req,
        CancellationToken ct)
    {
        var entity = await _db.Clientes.FirstOrDefaultAsync(c => c.IdCliente == id && !c.EsEliminado, ct);
        if (entity is null)
            return NotFound(ApiResponse<ClienteListItemDto>.Fail(404, "Cliente no encontrado.", HttpContext.TraceIdentifier));

        entity.TipoIdentificacion = ClientesApiMapper.ToDbTipoIdentificacion(req.TipoIdentificacion);
        entity.NumeroIdentificacion = req.NumeroIdentificacion.Trim();
        entity.CliNombre1 = req.Nombre1.Trim();
        entity.CliNombre2 = req.Nombre2?.Trim();
        entity.CliApellido1 = req.Apellido1.Trim();
        entity.CliApellido2 = req.Apellido2?.Trim();
        entity.FechaNacimiento = req.FechaNacimiento;
        entity.CliTelefono = req.Telefono.Trim();
        entity.CliCorreo = req.Correo.Trim();
        entity.DireccionPrincipal = req.DireccionPrincipal?.Trim();
        entity.ModificadoPorUsuario = "ADMIN_API";
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion = req.RowVersion > 0 ? req.RowVersion : entity.RowVersion + 1;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<ClienteListItemDto>.Ok(MapListItem(entity), "Cliente actualizado exitosamente", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct)
    {
        var entity = await _db.Clientes.FirstOrDefaultAsync(c => c.IdCliente == id && !c.EsEliminado, ct);
        if (entity is null)
            return NotFound(ApiResponse<object>.Fail(404, "Cliente no encontrado.", HttpContext.TraceIdentifier));

        entity.EsEliminado = true;
        entity.EstadoCliente = "INA";
        entity.ModificadoPorUsuario = "ADMIN_API";
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { id }, "Cliente eliminado exitosamente", HttpContext.TraceIdentifier));
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

            var saved = new Dictionary<string, ConductorUpsertResult>(conductores.Count, StringComparer.Ordinal);
            var pendingInsert = new List<Conductor>();

            foreach (var req in conductores)
            {
                var tipoDb = ClientesApiMapper.ToDbTipoIdentificacionConductor(req.TipoIdentificacion);
                var numero = (req.NumeroIdentificacion ?? string.Empty).Trim();
                var correo = req.Correo.Trim();
                var telefono = req.Telefono.Trim();
                var (n1, n2) = ClientesApiMapper.SplitTwo(req.Nombres);
                var (a1, a2) = ClientesApiMapper.SplitTwo(req.Apellidos);
                var edad = (short)Math.Clamp(req.EdadConductor, 21, 120);

                var existing = await _db.Conductores
                    .AsNoTracking()
                    .Where(c => c.IdCliente == idCliente && c.NumeroIdentificacion == numero && !c.EsEliminado)
                    .Select(c => new { c.IdConductor, c.ConductorGuid })
                    .FirstOrDefaultAsync(ct);

                if (existing is not null)
                {
                    await _db.Conductores
                        .Where(c => c.IdConductor == existing.IdConductor)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(c => c.ConNombre1, n1)
                            .SetProperty(c => c.ConNombre2, n2)
                            .SetProperty(c => c.ConApellido1, a1)
                            .SetProperty(c => c.ConApellido2, a2)
                            .SetProperty(c => c.FechaVencimientoLicencia, req.FechaVencimientoLicencia)
                            .SetProperty(c => c.EdadConductor, edad)
                            .SetProperty(c => c.ConCorreo, correo)
                            .SetProperty(c => c.ConTelefono, telefono),
                            ct);

                    saved[numero] = new ConductorUpsertResult
                    {
                        IdConductor = existing.IdConductor,
                        ConductorGuid = existing.ConductorGuid,
                        NumeroIdentificacion = numero,
                        EsPrincipal = req.EsPrincipal
                    };
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
                    ConTelefono = telefono,
                    ConCorreo = correo,
                    EstadoConductor = "ACT",
                    EsEliminado = false,
                    FechaRegistroUtc = DateTimeOffset.UtcNow,
                    CreadoPorUsuario = "BOOKING_API",
                    OrigenRegistro = "API"
                };

                pendingInsert.Add(entity);
            }

            if (pendingInsert.Count > 0)
            {
                _db.Conductores.AddRange(pendingInsert);
                await _db.SaveChangesAsync(ct);

                foreach (var entity in pendingInsert)
                {
                    saved[entity.NumeroIdentificacion] = new ConductorUpsertResult
                    {
                        IdConductor = entity.IdConductor,
                        ConductorGuid = entity.ConductorGuid,
                        NumeroIdentificacion = entity.NumeroIdentificacion,
                        EsPrincipal = conductores.First(c => (c.NumeroIdentificacion ?? string.Empty).Trim() == entity.NumeroIdentificacion).EsPrincipal
                    };
                }
            }

            var results = conductores
                .Select(req => saved[(req.NumeroIdentificacion ?? string.Empty).Trim()])
                .ToList();

            return Ok(ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Ok(results, traceId: HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IReadOnlyList<ConductorUpsertResult>>.Fail(
                500,
                ex.InnerException?.Message ?? ex.Message,
                HttpContext.TraceIdentifier));
        }
    }

    private static ClienteListItemDto MapListItem(Cliente c) => new()
    {
        IdCliente = c.IdCliente,
        ClienteGuid = c.ClienteGuid,
        CodigoCliente = c.CodigoCliente,
        TipoIdentificacion = ClientesApiMapper.ToApiTipoIdentificacion(c.TipoIdentificacion),
        NumeroIdentificacion = c.NumeroIdentificacion,
        Nombre1 = c.CliNombre1,
        Nombre2 = c.CliNombre2,
        Apellido1 = c.CliApellido1,
        Apellido2 = c.CliApellido2,
        NombreCompleto = ClientesApiMapper.JoinNames(
            ClientesApiMapper.JoinNames(c.CliNombre1, c.CliNombre2 ?? string.Empty),
            ClientesApiMapper.JoinNames(c.CliApellido1, c.CliApellido2 ?? string.Empty)),
        FechaNacimiento = c.FechaNacimiento,
        Telefono = c.CliTelefono,
        Correo = c.CliCorreo,
        DireccionPrincipal = c.DireccionPrincipal,
        EstadoCliente = c.EstadoCliente,
        RowVersion = c.RowVersion
    };

    private static ClienteDetalleDto MapDetalle(Cliente c) => new()
    {
        IdCliente = c.IdCliente,
        Nombres = ClientesApiMapper.JoinNames(c.CliNombre1, c.CliNombre2),
        Apellidos = ClientesApiMapper.JoinNames(c.CliApellido1, c.CliApellido2),
        TipoIdentificacion = ClientesApiMapper.ToApiTipoIdentificacion(c.TipoIdentificacion),
        NumeroIdentificacion = c.NumeroIdentificacion,
        Correo = c.CliCorreo,
        Telefono = c.CliTelefono
    };

    private static string BuildCodigoCliente(string tipoDb, string numero) =>
        Truncate($"CLI-{tipoDb}-{numero}", 20);

    private static string BuildCodigoConductor(string numero) =>
        Truncate($"CON-{numero}", 20);

    private static string BuildNumeroLicencia(string numero) =>
        Truncate($"LIC-{numero}", 30);

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
