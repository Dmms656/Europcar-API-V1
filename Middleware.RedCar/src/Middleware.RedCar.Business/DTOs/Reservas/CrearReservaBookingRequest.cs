namespace Middleware.RedCar.Business.DTOs.Reservas;

/// <summary>
/// Body del Endpoint 8 (POST /api/v2/booking/reservas). Forma EXACTA del contrato.
/// La validacion se hace con FluentValidation en CrearReservaValidator.
/// </summary>
public sealed class CrearReservaBookingRequest
{
    public int IdVehiculo { get; set; }
    public int IdLocalizacionRecogida { get; set; }
    public int IdLocalizacionDevolucion { get; set; }

    /// <summary>YYYY-MM-DD</summary>
    public DateOnly FechaInicio { get; set; }
    /// <summary>YYYY-MM-DD</summary>
    public DateOnly FechaFin { get; set; }

    /// <summary>HH:MM:SS</summary>
    public TimeOnly HoraInicio { get; set; }
    /// <summary>HH:MM:SS</summary>
    public TimeOnly HoraFin { get; set; }

    /// <summary>Maximo 300 caracteres.</summary>
    public string? Observaciones { get; set; }

    public ClienteReservaRequest Cliente { get; set; } = new();
    public List<ConductorReservaRequest> Conductores { get; set; } = new();
    public List<ExtraReservaRequest> Extras { get; set; } = new();
}

public sealed class ClienteReservaRequest
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;

    /// <summary>"CEDULA", "PASAPORTE", "RUC".</summary>
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}

public sealed class ConductorReservaRequest
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public DateOnly FechaVencimientoLicencia { get; set; }
    public int EdadConductor { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
}

public sealed class ExtraReservaRequest
{
    public int IdExtra { get; set; }
    public int Cantidad { get; set; }
}
