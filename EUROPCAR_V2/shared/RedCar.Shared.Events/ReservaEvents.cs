namespace RedCar.Shared.Events.Reservas;

public sealed record ReservaCreadaPayload(
    string CodigoReserva,
    string EstadoReserva,
    int IdVehiculo,
    int IdLocalizacionRecogida,
    int IdCliente,
    string FechaReservaUtc,
    int CantidadDias,
    double SubtotalVehiculo,
    double SubtotalExtras,
    double Subtotal,
    double Iva,
    double Total);

public sealed record ReservaCanceladaPayload(
    string CodigoReserva,
    string EstadoReserva,
    int IdVehiculo,
    string FechaCancelacionUtc);

public sealed record ReservaRechazadaPayload(
    string Motivo,
    int? IdVehiculo);

public sealed record DisponibilidadInvalidadaPayload(
    int IdVehiculo,
    int IdLocalizacion,
    string Razon);

public sealed record ProcesarReservaBookingCommand(
    Guid CorrelationId,
    int IdVehiculo,
    int IdLocalizacionRecogida,
    int IdLocalizacionDevolucion,
    string FechaInicio,
    string FechaFin,
    string HoraInicio,
    string HoraFin,
    string? Observaciones,
    string OrigenCanalReserva,
    ClienteBookingPayload Cliente,
    IReadOnlyList<ConductorBookingPayload> Conductores,
    IReadOnlyList<ExtraBookingPayload> Extras);

public sealed record ClienteBookingPayload(
    string Nombres,
    string Apellidos,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string Correo,
    string Telefono);

public sealed record ConductorBookingPayload(
    int IdConductor,
    string Nombres,
    string Apellidos,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string FechaVencimientoLicencia,
    int EdadConductor,
    string Correo,
    string Telefono,
    bool EsPrincipal);

public sealed record ExtraBookingPayload(int IdExtra, int Cantidad);

public sealed record UpsertClienteCommand(
    Guid CorrelationId,
    ClienteBookingPayload Cliente,
    IReadOnlyList<ConductorBookingPayload> Conductores);

public sealed record ClienteActualizadoPayload(
    Guid CorrelationId,
    int IdCliente,
    Guid ClienteGuid,
    bool Created,
    IReadOnlyList<ConductorRegistradoPayload> Conductores);

public sealed record ConductorRegistradoPayload(
    int IdConductor,
    string Nombres,
    string Apellidos,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string FechaVencimientoLicencia,
    int EdadConductor,
    string Correo,
    string Telefono,
    bool EsPrincipal);
