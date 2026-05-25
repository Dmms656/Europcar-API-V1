namespace Middleware.RedCar.DataAccess.Clients.Interfaces;

/// <summary>
/// Cliente REST hacia MS.Reservas (lecturas y escrituras transaccionales).
/// En Render el proxy publico no expone gRPC/HTTP2; crear/cancelar usan REST.
/// </summary>
public interface IReservasClient
{
    Task<DisponibilidadDto?> VerificarDisponibilidadAsync(int idVehiculo, int idLocalizacion, DateTimeOffset fechaRecogida, DateTimeOffset fechaDevolucion, CancellationToken ct = default);
    Task<ReservaDto?> GetReservaAsync(string codigoReserva, CancellationToken ct = default);
    Task<FacturaDto?> GetFacturaAsync(string codigoReserva, CancellationToken ct = default);
    Task<CrearReservaWriteResult> CrearReservaAsync(CrearReservaWriteRequest request, CancellationToken ct = default);
    Task<CancelarReservaWriteResult> CancelarReservaAsync(string codigoReserva, string motivo, string usuario, CancellationToken ct = default);

    Task<IReadOnlyList<ClienteReservaListItemDto>?> ListByClienteAsync(int idCliente, CancellationToken ct = default);

    Task<CancelarReservaWriteResult> CancelarByIdAsync(int idReserva, string motivo, string usuario, CancellationToken ct = default);
}

public sealed record ClienteReservaListItemDto(
    int IdReserva,
    Guid ReservaGuid,
    string CodigoReserva,
    string CodigoConfirmacion,
    string EstadoReserva,
    int IdCliente,
    int IdVehiculo,
    int IdLocalizacionRecogida,
    int IdLocalizacionDevolucion,
    string CanalReserva,
    DateTimeOffset FechaHoraRecogida,
    DateTimeOffset FechaHoraDevolucion,
    decimal Subtotal,
    decimal ValorImpuestos,
    decimal ValorExtras,
    decimal CargoOneWay,
    decimal Total,
    string? NombreCliente,
    string? PlacaVehiculo,
    string? DescripcionVehiculo,
    IReadOnlyList<ReservaExtraListItemDto> Extras);

public sealed record ReservaExtraListItemDto(
    int IdReservaExtra,
    int IdExtra,
    string CodigoExtra,
    string NombreExtra,
    int Cantidad,
    decimal ValorUnitario,
    decimal Subtotal);

public sealed record CrearReservaWriteRequest(
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
    CrearReservaWriteCliente Cliente,
    IReadOnlyList<CrearReservaWriteConductor> Conductores,
    IReadOnlyList<CrearReservaWriteExtra> Extras);

public sealed record CrearReservaWriteCliente(
    string Nombres, string Apellidos, string TipoIdentificacion,
    string NumeroIdentificacion, string Correo, string Telefono);

public sealed record CrearReservaWriteConductor(
    int IdConductor, string Nombres, string Apellidos, string TipoIdentificacion,
    string NumeroIdentificacion, DateOnly FechaVencimientoLicencia, int EdadConductor,
    string Correo, string Telefono, bool EsPrincipal);

public sealed record CrearReservaWriteExtra(int IdExtra, int Cantidad);

public sealed record CrearReservaWriteResult(
    string CodigoReserva, string EstadoReserva, DateTimeOffset FechaReservaUtc,
    int CantidadDias, decimal SubtotalVehiculo, decimal SubtotalExtras,
    decimal Subtotal, decimal Iva, decimal Total);

public sealed record CancelarReservaWriteResult(
    string CodigoReserva, string EstadoReserva, DateTimeOffset FechaCancelacionUtc);

public sealed record DisponibilidadDto(
    int IdVehiculo,
    int IdLocalizacion,
    DateTimeOffset FechaRecogida,
    DateTimeOffset FechaDevolucion,
    bool Disponible);

public sealed record ReservaDto(
    string CodigoReserva,
    string EstadoReserva,
    string OrigenCanalReserva,
    DateTimeOffset FechaReservaUtc,
    DateTimeOffset? FechaConfirmacionUtc,
    DateTimeOffset? FechaCancelacionUtc,
    string? MotivoCancelacion,
    string? Observaciones,
    ReservaVehiculoDto Vehiculo,
    ReservaLocalizacionDto LocalizacionRecogida,
    ReservaLocalizacionDto LocalizacionDevolucion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    int CantidadDias,
    ReservaClienteDto Cliente,
    IReadOnlyList<ReservaConductorDto> Conductores,
    IReadOnlyList<ReservaExtraDto> Extras,
    decimal SubtotalVehiculo,
    decimal SubtotalExtras,
    decimal Subtotal,
    decimal Iva,
    decimal Total);

public sealed record ReservaVehiculoDto(int IdVehiculo, string CodigoInterno, string Marca, string Modelo);
public sealed record ReservaLocalizacionDto(int IdLocalizacion, string Nombre);
public sealed record ReservaClienteDto(string Nombres, string Apellidos, string TipoIdentificacion, string NumeroIdentificacion, string Correo, string Telefono);
public sealed record ReservaConductorDto(string Nombres, string Apellidos, string TipoIdentificacion, string NumeroIdentificacion, int EdadConductor, bool EsPrincipal);
public sealed record ReservaExtraDto(int IdExtra, string Nombre, int Cantidad, decimal ValorUnitario, decimal Subtotal);

public sealed record FacturaDto(
    string NumeroFactura,
    string CodigoReserva,
    DateTimeOffset FechaFacturaUtc,
    decimal Subtotal,
    decimal Iva,
    decimal Total,
    string Moneda,
    string? UrlPdf);
