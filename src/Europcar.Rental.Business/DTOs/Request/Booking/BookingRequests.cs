namespace Europcar.Rental.Business.DTOs.Request.Booking;

/// <summary>
/// Parámetros de búsqueda de vehículos para el API de Booking / OTA.
/// Mapea los query-params definidos en el contrato de API.
/// </summary>
public class BookingBuscarVehiculosRequest
{
    /// <summary>ID de la localización de recogida (obligatorio).</summary>
    public int IdLocalizacion { get; set; }

    /// <summary>Fecha y hora de recogida en ISO 8601.</summary>
    public DateTimeOffset FechaRecogida { get; set; }

    /// <summary>Fecha y hora de devolución en ISO 8601.</summary>
    public DateTimeOffset FechaDevolucion { get; set; }

    /// <summary>Filtrar por categoría de vehículo (opcional).</summary>
    public int? IdCategoria { get; set; }

    /// <summary>Filtrar por marca (opcional).</summary>
    public int? IdMarca { get; set; }

    /// <summary>Tipo de transmisión: AUTOMATICA, MANUAL (opcional).</summary>
    public string? Transmision { get; set; }

    /// <summary>Criterio de orden: precio_asc, precio_desc. Default: precio_asc.</summary>
    public string Sort { get; set; } = "precio_asc";

    /// <summary>Número de página (base 1). Default: 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Cantidad de resultados por página. Default: 20, máx: 100.</summary>
    public int Limit { get; set; } = 20;
}

/// <summary>
/// Parámetros para verificar la disponibilidad en tiempo real de un vehículo.
/// </summary>
public class BookingDisponibilidadRequest
{
    public DateTimeOffset FechaRecogida { get; set; }
    public DateTimeOffset FechaDevolucion { get; set; }
    public int IdLocalizacion { get; set; }
}

/// <summary>
/// Parámetros para listar localizaciones con filtro opcional de ciudad.
/// </summary>
public class BookingLocalizacionesRequest
{
    public int? IdCiudad { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}
