namespace Europcar.Rental.Business.DTOs.Response.Booking;

// =====================================================
// Wrapper genérico para endpoints Booking (formato del contrato API)
// =====================================================
public class BookingResponse<T>
{
    public int Status { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static BookingResponse<T> Ok(T data, string mensaje = "Operación exitosa") => new()
    {
        Status = 200,
        Mensaje = mensaje,
        Data = data
    };

    public static BookingResponse<T> Fail(int status, string mensaje) => new()
    {
        Status = status,
        Mensaje = mensaje,
        Data = default
    };
}

// =====================================================
// Paginación y HATEOAS _links
// =====================================================
public class PaginacionDto
{
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalElementos { get; set; }
    public int ElementosPorPagina { get; set; }
}

public class LinkDto
{
    public string Href { get; set; } = string.Empty;
}

// =====================================================
// Endpoint 1 & 2 — Vehículos
// =====================================================
public class BookingVehiculoListData
{
    public List<BookingVehiculoResponse> Vehiculos { get; set; } = new();
    public PaginacionDto Paginacion { get; set; } = new();
    public Dictionary<string, LinkDto>? _links { get; set; }
}

public class BookingVehiculoDetailData
{
    public BookingVehiculoResponse Vehiculo { get; set; } = null!;
}

public class BookingVehiculoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string MarcaModelo { get; set; } = string.Empty;
    public short Anio { get; set; }
    public string? ImagenUrl { get; set; }
    public string Transmision { get; set; } = string.Empty;
    public string Combustible { get; set; } = string.Empty;
    public short CapacidadPasajeros { get; set; }
    public short CapacidadMaletas { get; set; }
    public short NumeroPuertas { get; set; }
    public bool AireAcondicionado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public BookingCategoriaDto? Categoria { get; set; }
    public BookingLocalizacionCorta Localizacion { get; set; } = new();
    public BookingDisponibilidadDto? Disponibilidad { get; set; }
    public BookingPrecioDto Precio { get; set; } = new();
    public List<BookingExtraCorto> ExtrasDisponibles { get; set; } = new();
    public Dictionary<string, LinkDto>? _links { get; set; }
}

public class BookingCategoriaDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public class BookingLocalizacionCorta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
}

public class BookingDisponibilidadDto
{
    public string FechaInicio { get; set; } = string.Empty;
    public string FechaFin { get; set; } = string.Empty;
    public bool Disponible { get; set; }
}

public class BookingPrecioDto
{
    public decimal MontoBase { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
}

public class BookingExtraCorto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}

// =====================================================
// Endpoint 3 — Disponibilidad en tiempo real
// =====================================================
public class BookingDisponibilidadCheckData
{
    public string VehiculoId { get; set; } = string.Empty;
    public BookingDisponibilidadDto Disponibilidad { get; set; } = new();
}

// =====================================================
// Endpoint 4 & 5 — Localizaciones
// =====================================================
public class BookingLocalizacionListData
{
    public List<BookingLocalizacionResponse> Localizaciones { get; set; } = new();
    public PaginacionDto Paginacion { get; set; } = new();
    public Dictionary<string, LinkDto>? _links { get; set; }
}

public class BookingLocalizacionDetailData
{
    public BookingLocalizacionResponse Localizacion { get; set; } = null!;
}

public class BookingLocalizacionResponse
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string HorarioAtencion { get; set; } = string.Empty;
    public string? ZonaHoraria { get; set; }
    public BookingCiudadDto Ciudad { get; set; } = new();
    public Dictionary<string, LinkDto>? _links { get; set; }
}

public class BookingCiudadDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

// =====================================================
// Endpoint 6 — Categorías
// =====================================================
public class BookingCategoriaListData
{
    public List<BookingCategoriaResponse> Categorias { get; set; } = new();
}

public class BookingCategoriaResponse
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

// =====================================================
// Endpoint 7 — Extras
// =====================================================
public class BookingExtraListData
{
    public List<BookingExtraResponse> Extras { get; set; } = new();
}

public class BookingExtraResponse
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal ValorFijo { get; set; }
}
