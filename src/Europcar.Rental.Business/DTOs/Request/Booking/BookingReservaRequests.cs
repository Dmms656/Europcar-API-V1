namespace Europcar.Rental.Business.DTOs.Request.Booking;

/// <summary>
/// Cuerpo de la solicitud de creación de reserva publicada por el contrato Booking
/// (Endpoint 8 — POST /api/v1/reservas).
/// Mapea exactamente los campos del documento Contrato_API_Vehiculos_RedCar_V1.
/// </summary>
public class BookingCrearReservaRequest
{
    /// <summary>Identificador alfanumérico del vehículo (CodigoInternoVehiculo).</summary>
    public string IdVehiculo { get; set; } = string.Empty;

    public int IdLocalizacionRecogida { get; set; }
    public int IdLocalizacionEntrega { get; set; }

    /// <summary>Fecha de inicio del alquiler (yyyy-MM-dd).</summary>
    public DateOnly FechaInicio { get; set; }

    /// <summary>Fecha de fin del alquiler (yyyy-MM-dd).</summary>
    public DateOnly FechaFin { get; set; }

    /// <summary>Hora de recogida (HH:mm:ss). Si no se envía, se asume 09:00.</summary>
    public TimeOnly? HoraInicio { get; set; }

    /// <summary>Hora de devolución (HH:mm:ss). Si no se envía, se asume 09:00.</summary>
    public TimeOnly? HoraFin { get; set; }

    /// <summary>Canal de origen (BOOKING, OTA, WEB...). Default BOOKING.</summary>
    public string OrigenCanalReserva { get; set; } = "BOOKING";

    public BookingClienteData Cliente { get; set; } = new();
    public BookingConductorData ConductorPrincipal { get; set; } = new();
    public BookingConductorData? ConductorSecundario { get; set; }
    public List<BookingReservaExtraItem> Extras { get; set; } = new();
}

public class BookingClienteData
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = "CED";
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}

public class BookingConductorData : BookingClienteData
{
    public string NumeroLicencia { get; set; } = string.Empty;
    public DateOnly? FechaVencimientoLicencia { get; set; }
    public short EdadConductor { get; set; }
}

public class BookingReservaExtraItem
{
    public int IdExtra { get; set; }
    public int Cantidad { get; set; } = 1;
}

/// <summary>
/// Cuerpo de la cancelación pública de reserva (Endpoint 10 — PATCH).
/// </summary>
public class BookingCancelarReservaRequest
{
    public string MotivoCancelacion { get; set; } = string.Empty;
}
