namespace RedCar.Reservas.Api.Contracts;

/// <summary>
/// Body REST para crear reserva (misma forma que gRPC CrearReservaRequest; usable desde Render sin HTTP/2).
/// </summary>
public sealed class CrearReservaRestRequest
{
    public int IdVehiculo { get; set; }
    public int IdLocalizacionRecogida { get; set; }
    public int IdLocalizacionDevolucion { get; set; }
    public string FechaInicio { get; set; } = string.Empty;
    public string FechaFin { get; set; } = string.Empty;
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? OrigenCanalReserva { get; set; }
    public int IdCliente { get; set; }
    public CrearReservaClienteRestDto Cliente { get; set; } = new();
    public IReadOnlyList<CrearReservaConductorRestDto> Conductores { get; set; } = Array.Empty<CrearReservaConductorRestDto>();
    public IReadOnlyList<CrearReservaExtraRestDto> Extras { get; set; } = Array.Empty<CrearReservaExtraRestDto>();
}

public sealed class CrearReservaClienteRestDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}

public sealed class CrearReservaConductorRestDto
{
    public int IdConductor { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string FechaVencimientoLicencia { get; set; } = string.Empty;
    public int EdadConductor { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
}

public sealed class CrearReservaExtraRestDto
{
    public int IdExtra { get; set; }
    public int Cantidad { get; set; }
}

public sealed class CrearReservaRestResponse
{
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaReservaUtc { get; set; }
    public int CantidadDias { get; set; }
    public decimal SubtotalVehiculo { get; set; }
    public decimal SubtotalExtras { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}

public sealed class CancelarReservaRestRequest
{
    public string MotivoCancelacion { get; set; } = string.Empty;
    public string? UsuarioCancelacion { get; set; }
}

public sealed class CancelarReservaRestResponse
{
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaCancelacionUtc { get; set; }
}
