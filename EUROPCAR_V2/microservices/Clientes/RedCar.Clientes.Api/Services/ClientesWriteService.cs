using Microsoft.EntityFrameworkCore;
using RedCar.Clientes.Api.Contracts;
using RedCar.Clientes.DataAccess.Context;
using RedCar.Clientes.DataAccess.Entities;
using RedCar.Shared.Events.Reservas;

namespace RedCar.Clientes.Api.Services;

public sealed class ClientesWriteService
{
    private readonly ClientesDbContext _db;

    public ClientesWriteService(ClientesDbContext db) => _db = db;

    public async Task<(ClienteUpsertResult Cliente, IReadOnlyList<ConductorUpsertResult> Conductores)> UpsertBookingAsync(
        ClienteBookingPayload cliente,
        IReadOnlyList<ConductorBookingPayload> conductores,
        CancellationToken ct)
    {
        var tipoDb = ClientesApiMapper.ToDbTipoIdentificacion(cliente.TipoIdentificacion);
        var numero = cliente.NumeroIdentificacion.Trim();
        var (n1, n2) = ClientesApiMapper.SplitTwo(cliente.Nombres);
        var (a1, a2) = ClientesApiMapper.SplitTwo(cliente.Apellidos);
        var correo = cliente.Correo.Trim();
        var telefono = cliente.Telefono.Trim();

        var existing = await _db.Clientes
            .FirstOrDefaultAsync(c => c.NumeroIdentificacion == numero && !c.EsEliminado, ct);

        ClienteUpsertResult result;
        if (existing is not null)
        {
            existing.CliNombre1 = n1;
            existing.CliNombre2 = n2;
            existing.CliApellido1 = a1;
            existing.CliApellido2 = a2;
            existing.CliCorreo = correo;
            existing.CliTelefono = telefono;
            await _db.SaveChangesAsync(ct);
            result = new ClienteUpsertResult { IdCliente = existing.IdCliente, ClienteGuid = existing.ClienteGuid, Created = false };
        }
        else
        {
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
                EstadoCliente = "ACT",
                EsEliminado = false,
                FechaRegistroUtc = DateTimeOffset.UtcNow,
                CreadoPorUsuario = "BOOKING_EvB",
                OrigenRegistro = "API"
            };
            _db.Clientes.Add(entity);
            await _db.SaveChangesAsync(ct);
            result = new ClienteUpsertResult { IdCliente = entity.IdCliente, ClienteGuid = entity.ClienteGuid, Created = true };
        }

        var conductorResults = await UpsertConductoresInternalAsync(result.IdCliente, conductores, ct);
        return (result, conductorResults);
    }

    private async Task<IReadOnlyList<ConductorUpsertResult>> UpsertConductoresInternalAsync(
        int idCliente,
        IReadOnlyList<ConductorBookingPayload> conductores,
        CancellationToken ct)
    {
        if (conductores.Count == 0)
            return Array.Empty<ConductorUpsertResult>();

        var saved = new Dictionary<string, ConductorUpsertResult>(conductores.Count, StringComparer.Ordinal);
        var pendingInsert = new List<Conductor>();

        foreach (var req in conductores)
        {
            var tipoDb = ClientesApiMapper.ToDbTipoIdentificacionConductor(req.TipoIdentificacion);
            var numero = req.NumeroIdentificacion.Trim();
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
                        .SetProperty(c => c.FechaVencimientoLicencia, DateOnly.Parse(req.FechaVencimientoLicencia))
                        .SetProperty(c => c.EdadConductor, edad)
                        .SetProperty(c => c.ConCorreo, req.Correo.Trim())
                        .SetProperty(c => c.ConTelefono, req.Telefono.Trim()),
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

            pendingInsert.Add(new Conductor
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
                FechaVencimientoLicencia = DateOnly.Parse(req.FechaVencimientoLicencia),
                EdadConductor = edad,
                ConTelefono = req.Telefono.Trim(),
                ConCorreo = req.Correo.Trim(),
                EstadoConductor = "ACT",
                EsEliminado = false,
                FechaRegistroUtc = DateTimeOffset.UtcNow,
                CreadoPorUsuario = "BOOKING_EvB",
                OrigenRegistro = "API"
            });
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
                    EsPrincipal = conductores.First(c => c.NumeroIdentificacion.Trim() == entity.NumeroIdentificacion).EsPrincipal
                };
            }
        }

        return conductores.Select(req => saved[req.NumeroIdentificacion.Trim()]).ToList();
    }

    private static string BuildCodigoCliente(string tipoDb, string numero) =>
        $"{tipoDb}-{numero}".ToUpperInvariant();

    private static string BuildCodigoConductor(string numero) => $"COND-{numero}".ToUpperInvariant();

    private static string BuildNumeroLicencia(string numero) => $"LIC-{numero}";
}
