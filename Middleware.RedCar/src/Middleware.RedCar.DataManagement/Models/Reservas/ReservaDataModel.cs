namespace Middleware.RedCar.DataManagement.Models.Reservas;

public sealed record ReservaDataModel
{
    public required string CodigoReserva { get; init; }
    public required string EstadoReserva { get; init; }
    public required string OrigenCanalReserva { get; init; }
    public required DateTimeOffset FechaReservaUtc { get; init; }
    public DateTimeOffset? FechaConfirmacionUtc { get; init; }
    public DateTimeOffset? FechaCancelacionUtc { get; init; }
    public string? MotivoCancelacion { get; init; }
    public string? Observaciones { get; init; }

    public required ReservaVehiculoData Vehiculo { get; init; }
    public required ReservaLocalizacionData LocalizacionRecogida { get; init; }
    public required ReservaLocalizacionData LocalizacionDevolucion { get; init; }

    public required DateOnly FechaInicio { get; init; }
    public required DateOnly FechaFin { get; init; }
    public required TimeOnly HoraInicio { get; init; }
    public required TimeOnly HoraFin { get; init; }
    public required int CantidadDias { get; init; }

    public required ReservaClienteData Cliente { get; init; }
    public required IReadOnlyList<ReservaConductorData> Conductores { get; init; }
    public required IReadOnlyList<ReservaExtraData> Extras { get; init; }

    public required decimal SubtotalVehiculo { get; init; }
    public required decimal SubtotalExtras { get; init; }
    public required decimal Subtotal { get; init; }
    public required decimal Iva { get; init; }
    public required decimal Total { get; init; }
}

public sealed record ReservaVehiculoData(int IdVehiculo, string CodigoInterno, string Marca, string Modelo);
public sealed record ReservaLocalizacionData(int IdLocalizacion, string Nombre);
public sealed record ReservaClienteData(string Nombres, string Apellidos, string TipoIdentificacion, string NumeroIdentificacion, string Correo, string Telefono);
public sealed record ReservaConductorData(string Nombres, string Apellidos, string TipoIdentificacion, string NumeroIdentificacion, int EdadConductor, bool EsPrincipal);
public sealed record ReservaExtraData(int IdExtra, string Nombre, int Cantidad, decimal ValorUnitario, decimal Subtotal);
