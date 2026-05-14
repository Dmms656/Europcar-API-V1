namespace Middleware.RedCar.DataAccess.Clients.Interfaces;

/// <summary>
/// Cliente REST hacia MS.Reservas (esquema "reservas").
/// Las queries (consultar reserva, factura, disponibilidad) van por REST.
/// La operacion de crear reserva, por su naturaleza transaccional, viaja por gRPC
/// (ver IReservasGrpcClient).
/// </summary>
public interface IReservasClient
{
    Task<DisponibilidadDto?> VerificarDisponibilidadAsync(int idVehiculo, int idLocalizacion, DateTimeOffset fechaRecogida, DateTimeOffset fechaDevolucion, CancellationToken ct = default);
    Task<ReservaDto?> GetReservaAsync(string codigoReserva, CancellationToken ct = default);
    Task<FacturaDto?> GetFacturaAsync(string codigoReserva, CancellationToken ct = default);
}

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
