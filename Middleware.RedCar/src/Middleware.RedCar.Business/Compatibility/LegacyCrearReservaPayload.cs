using System.Text.Json.Serialization;
using Middleware.RedCar.Business.DTOs.Reservas;

namespace Middleware.RedCar.Business.Compatibility;

/// <summary>
/// Cuerpo legacy compatible con el frontend React y con el monolito
/// <c>BookingCrearReservaRequest</c> (POST /api/v1/reservas).
/// </summary>
public sealed class LegacyCrearReservaPayload
{
    /// <summary>ID numérico o, en el futuro, código interno resuelto por catálogo.</summary>
    public string IdVehiculo { get; set; } = string.Empty;

    public int IdLocalizacionRecogida { get; set; }

    [JsonPropertyName("idLocalizacionEntrega")]
    public int? IdLocalizacionEntrega { get; set; }

    [JsonPropertyName("idLocalizacionDevolucion")]
    public int? IdLocalizacionDevolucion { get; set; }

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public string? Observaciones { get; set; }

    /// <summary>WEB, BOOKING, etc. El contrato RedCar V2 exige BOOKING para canal externo.</summary>
    public string? OrigenCanalReserva { get; set; }

    public ClienteReservaRequest Cliente { get; set; } = new();

    public ConductorLegacyPayload? ConductorPrincipal { get; set; }
    public ConductorLegacyPayload? ConductorSecundario { get; set; }

    public List<ConductorLegacyPayload>? Conductores { get; set; }

    public List<ExtraReservaRequest>? Extras { get; set; }
}

public sealed class ConductorLegacyPayload
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = "CEDULA";
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? NumeroLicencia { get; set; }
    public DateOnly? FechaVencimientoLicencia { get; set; }
    public int EdadConductor { get; set; }
    public bool EsPrincipal { get; set; }
}
