namespace Europcar.Rental.Business.DTOs.Response.Booking;

// =====================================================
// Endpoint 8 — Crear reserva (POST)
// =====================================================
public class BookingCrearReservaData
{
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaReservaUtc { get; set; }
    public BookingVehiculoCorto Vehiculo { get; set; } = new();
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int CantidadDias { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public Dictionary<string, LinkDto>? _links { get; set; }
}

// =====================================================
// Endpoint 9 — Detalle de reserva (GET)
// =====================================================
public class BookingReservaDetailData
{
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public string OrigenCanal { get; set; } = string.Empty;
    public DateTimeOffset FechaReservaUtc { get; set; }
    public BookingVehiculoCorto Vehiculo { get; set; } = new();
    public BookingLocalizacionMini LocalizacionRecogida { get; set; } = new();
    public BookingLocalizacionMini LocalizacionEntrega { get; set; } = new();
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int CantidadDias { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public List<BookingReservaExtraDto> Extras { get; set; } = new();
    public Dictionary<string, LinkDto>? _links { get; set; }
}

public class BookingVehiculoCorto
{
    public string Id { get; set; } = string.Empty;
    public string MarcaModelo { get; set; } = string.Empty;
}

public class BookingLocalizacionMini
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class BookingReservaExtraDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

// =====================================================
// Endpoint 10 — Cancelar reserva (PATCH)
// =====================================================
public class BookingCancelarReservaData
{
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaCancelacionUtc { get; set; }
    public string MotivoCancelacion { get; set; } = string.Empty;
}

// =====================================================
// Endpoint 11 — Factura asociada a la reserva (GET)
// =====================================================
public class BookingFacturaData
{
    public BookingFacturaDto Factura { get; set; } = new();
}

public class BookingFacturaDto
{
    public string NumeroFactura { get; set; } = string.Empty;
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoFactura { get; set; } = string.Empty;
    public DateTimeOffset FechaEmision { get; set; }
    public BookingClienteFacturaDto Cliente { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}

public class BookingClienteFacturaDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
}
