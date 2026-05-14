namespace Middleware.RedCar.DataAccess.Clients.Interfaces;

/// <summary>
/// Cliente REST hacia MS.Clientes (esquema "clientes").
/// </summary>
public interface IClientesClient
{
    /// <summary>
    /// Upsert de cliente por (tipoIdentificacion, numeroIdentificacion).
    /// Si existe, devuelve el id existente; si no, lo crea.
    /// </summary>
    Task<ClienteUpsertResult?> UpsertClienteAsync(ClienteUpsertRequest req, CancellationToken ct = default);

    /// <summary>
    /// Upsert masivo de conductores asociados a un cliente.
    /// </summary>
    Task<IReadOnlyList<ConductorUpsertResult>?> UpsertConductoresAsync(int idCliente, IReadOnlyList<ConductorUpsertRequest> conductores, CancellationToken ct = default);
}

public sealed record ClienteUpsertRequest(
    string Nombres,
    string Apellidos,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string Correo,
    string Telefono);

public sealed record ClienteUpsertResult(
    int IdCliente,
    Guid ClienteGuid,
    bool Created);

public sealed record ConductorUpsertRequest(
    string Nombres,
    string Apellidos,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    DateOnly FechaVencimientoLicencia,
    int EdadConductor,
    string Correo,
    string Telefono,
    bool EsPrincipal);

public sealed record ConductorUpsertResult(
    int IdConductor,
    Guid ConductorGuid,
    string NumeroIdentificacion,
    bool EsPrincipal);
