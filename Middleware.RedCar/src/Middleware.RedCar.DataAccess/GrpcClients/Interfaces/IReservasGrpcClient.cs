namespace Middleware.RedCar.DataAccess.GrpcClients.Interfaces;

/// <summary>
/// Fachada del cliente gRPC hacia MS.Reservas para operaciones transaccionales
/// (crear reserva, cancelar reserva). Mantenemos una interfaz propia para poder
/// mockearla en pruebas y desacoplar el orquestador del generated client de Grpc.Tools.
/// </summary>
public interface IReservasGrpcClient
{
    Task<CrearReservaGrpcResult> CrearReservaAsync(CrearReservaGrpcRequest request, CancellationToken ct = default);
    Task<CancelarReservaGrpcResult> CancelarReservaAsync(string codigoReserva, string motivo, string usuario, CancellationToken ct = default);
}

public sealed record CrearReservaGrpcRequest(
    int IdVehiculo,
    int IdLocalizacionRecogida,
    int IdLocalizacionDevolucion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string? Observaciones,
    string OrigenCanalReserva,
    int IdCliente,
    CrearReservaGrpcCliente Cliente,
    IReadOnlyList<CrearReservaGrpcConductor> Conductores,
    IReadOnlyList<CrearReservaGrpcExtra> Extras);

public sealed record CrearReservaGrpcCliente(
    string Nombres,
    string Apellidos,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string Correo,
    string Telefono);

public sealed record CrearReservaGrpcConductor(
    int IdConductor,
    string Nombres,
    string Apellidos,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    DateOnly FechaVencimientoLicencia,
    int EdadConductor,
    string Correo,
    string Telefono,
    bool EsPrincipal);

public sealed record CrearReservaGrpcExtra(int IdExtra, int Cantidad);

public sealed record CrearReservaGrpcResult(
    string CodigoReserva,
    string EstadoReserva,
    DateTimeOffset FechaReservaUtc,
    int CantidadDias,
    decimal SubtotalVehiculo,
    decimal SubtotalExtras,
    decimal Subtotal,
    decimal Iva,
    decimal Total);

public sealed record CancelarReservaGrpcResult(
    string CodigoReserva,
    string EstadoReserva,
    DateTimeOffset FechaCancelacionUtc);
