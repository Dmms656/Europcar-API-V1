namespace RedCar.Reservas.Api.Contracts;

public sealed class DisponibilidadDto
{
    public int IdVehiculo { get; set; }
    public int IdLocalizacion { get; set; }
    public DateTimeOffset FechaRecogida { get; set; }
    public DateTimeOffset FechaDevolucion { get; set; }
    public bool Disponible { get; set; }
}

public sealed class ReservaDto
{
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public string OrigenCanalReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaReservaUtc { get; set; }
    public DateTimeOffset? FechaConfirmacionUtc { get; set; }
    public DateTimeOffset? FechaCancelacionUtc { get; set; }
    public string? MotivoCancelacion { get; set; }
    public string? Observaciones { get; set; }
    public ReservaVehiculoDto Vehiculo { get; set; } = new();
    public ReservaLocalizacionDto LocalizacionRecogida { get; set; } = new();
    public ReservaLocalizacionDto LocalizacionDevolucion { get; set; } = new();
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public int CantidadDias { get; set; }
    public ReservaClienteDto Cliente { get; set; } = new();
    public IReadOnlyList<ReservaConductorDto> Conductores { get; set; } = Array.Empty<ReservaConductorDto>();
    public IReadOnlyList<ReservaExtraDto> Extras { get; set; } = Array.Empty<ReservaExtraDto>();
    public decimal SubtotalVehiculo { get; set; }
    public decimal SubtotalExtras { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}

public sealed class ReservaVehiculoDto
{
    public int IdVehiculo { get; set; }
    public string CodigoInterno { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
}

public sealed class ReservaLocalizacionDto
{
    public int IdLocalizacion { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public sealed class ReservaClienteDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}

public sealed class ReservaConductorDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public int EdadConductor { get; set; }
    public bool EsPrincipal { get; set; }
}

public sealed class ReservaExtraDto
{
    public int IdExtra { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public sealed class FacturaDto
{
    public string NumeroFactura { get; set; } = string.Empty;
    public string CodigoReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaFacturaUtc { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "USD";
    public string? UrlPdf { get; set; }
}
