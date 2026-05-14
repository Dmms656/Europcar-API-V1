using System.Text.Json.Serialization;
using Middleware.RedCar.Business.DTOs.Booking;

namespace Middleware.RedCar.Business.DTOs.Reservas;

/// <summary>
/// Response del Endpoint 8 (POST crear reserva). Forma EXACTA del contrato (201).
/// </summary>
public sealed class CrearReservaBookingResponse
{
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaReservaUtc { get; set; }
    public VehiculoReservaResumen Vehiculo { get; set; } = new();
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public int CantidadDias { get; set; }
    public decimal SubtotalVehiculo { get; set; }
    public decimal SubtotalExtras { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }

    [JsonPropertyName("_links")]
    public Dictionary<string, LinkHref> _Links { get; set; } = new();
}

/// <summary>
/// Response del Endpoint 9 (GET reserva detalle). Forma EXACTA del contrato.
/// </summary>
public sealed class ReservaBookingResponse
{
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public string OrigenCanalReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaReservaUtc { get; set; }
    public DateTimeOffset? FechaConfirmacionUtc { get; set; }
    public DateTimeOffset? FechaCancelacionUtc { get; set; }
    public string? MotivoCancelacion { get; set; }
    public string? Observaciones { get; set; }

    public VehiculoReservaResumen Vehiculo { get; set; } = new();
    public LocalizacionResumenReserva LocalizacionRecogida { get; set; } = new();
    public LocalizacionResumenReserva LocalizacionDevolucion { get; set; } = new();

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public int CantidadDias { get; set; }

    public ClienteReservaResponse Cliente { get; set; } = new();
    public List<ConductorReservaResponse> Conductores { get; set; } = new();
    public List<ExtraReservaResponse> Extras { get; set; } = new();

    public decimal SubtotalVehiculo { get; set; }
    public decimal SubtotalExtras { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }

    [JsonPropertyName("_links")]
    public Dictionary<string, LinkHref> _Links { get; set; } = new();
}

public sealed class VehiculoReservaResumen
{
    public int IdVehiculo { get; set; }
    public string CodigoInterno { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
}

public sealed class LocalizacionResumenReserva
{
    public int IdLocalizacion { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public sealed class ClienteReservaResponse
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}

public sealed class ConductorReservaResponse
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public int EdadConductor { get; set; }
    public bool EsPrincipal { get; set; }
}

public sealed class ExtraReservaResponse
{
    public int IdExtra { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
