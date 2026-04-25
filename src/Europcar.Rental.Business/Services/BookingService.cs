using Europcar.Rental.Business.DTOs.Request.Booking;
using Europcar.Rental.Business.DTOs.Response.Booking;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.Business.Services;

/// <summary>
/// Implementación del servicio de Booking / OTA.
/// Orquesta las consultas de disponibilidad, vehículos, localizaciones,
/// categorías y extras según el contrato de API externo.
/// Tasa de impuesto IVA fija: 15 % (regla de negocio Ecuador).
/// </summary>
public class BookingService : IBookingService
{
    private readonly IVehiculoDataService _vehiculoDataService;
    private readonly IReservaDataService _reservaDataService;
    private readonly IBookingDataService _bookingDataService;
    private readonly ICatalogoDataService _catalogoDataService;

    private const decimal TasaIva = 0.15m;

    public BookingService(
        IVehiculoDataService vehiculoDataService,
        IReservaDataService reservaDataService,
        IBookingDataService bookingDataService,
        ICatalogoDataService catalogoDataService)
    {
        _vehiculoDataService = vehiculoDataService;
        _reservaDataService = reservaDataService;
        _bookingDataService = bookingDataService;
        _catalogoDataService = catalogoDataService;
    }

    // =====================================================
    // Endpoint 1: Búsqueda paginada de vehículos disponibles
    // =====================================================
    public async Task<BookingResponse<BookingVehiculoListData>> BuscarVehiculosAsync(BookingBuscarVehiculosRequest request)
    {
        // Normalizar paginación
        if (request.Page < 1) request.Page = 1;
        if (request.Limit < 1) request.Limit = 20;
        if (request.Limit > 100) request.Limit = 100;

        // Obtener vehículos disponibles en la localización y categoría
        var todos = await _vehiculoDataService.GetDisponiblesAsync(request.IdLocalizacion, request.IdCategoria);
        var lista = todos.ToList();

        // Filtro por transmisión
        if (!string.IsNullOrWhiteSpace(request.Transmision))
            lista = lista.Where(v => v.TipoTransmision.Equals(request.Transmision, StringComparison.OrdinalIgnoreCase)).ToList();

        // Excluir vehículos con reservas solapadas en las fechas solicitadas
        var disponibles = new List<VehiculoModel>();
        foreach (var v in lista)
        {
            var solapado = await _reservaDataService.ExisteSolapamientoAsync(
                v.IdVehiculo, request.FechaRecogida, request.FechaDevolucion);
            if (!solapado)
                disponibles.Add(v);
        }

        // Calcular días de alquiler (mínimo 1)
        var dias = Math.Max(1, (int)Math.Ceiling((request.FechaDevolucion - request.FechaRecogida).TotalDays));

        // Ordenar por precio
        disponibles = request.Sort?.ToLowerInvariant() == "precio_desc"
            ? disponibles.OrderByDescending(v => v.PrecioBaseDia).ToList()
            : disponibles.OrderBy(v => v.PrecioBaseDia).ToList();

        // Paginación
        var totalElementos = disponibles.Count;
        var totalPaginas = (int)Math.Ceiling((double)totalElementos / request.Limit);
        var paginados = disponibles.Skip((request.Page - 1) * request.Limit).Take(request.Limit).ToList();

        // Obtener extras disponibles con precios reales
        var extrasGlobal = (await _bookingDataService.GetExtrasConPrecioAsync()).ToList();

        // Mapear al formato del contrato
        var vehiculosDto = paginados.Select(v =>
        {
            var montoBase = v.PrecioBaseDia * dias;
            var impuestos = Math.Round(montoBase * TasaIva, 2);
            return MapVehiculoResponse(v, montoBase, impuestos, extrasGlobal,
                request.FechaRecogida, request.FechaDevolucion, incluirCategoria: false);
        }).ToList();

        var data = new BookingVehiculoListData
        {
            Vehiculos = vehiculosDto,
            Paginacion = new PaginacionDto
            {
                PaginaActual = request.Page,
                TotalPaginas = totalPaginas,
                TotalElementos = totalElementos,
                ElementosPorPagina = request.Limit
            },
            _links = BuildPaginationLinks("/api/v1/vehiculos", request.Page, totalPaginas, request.Limit)
        };

        return BookingResponse<BookingVehiculoListData>.Ok(data);
    }

    // =====================================================
    // Endpoint 2: Detalle completo de un vehículo
    // =====================================================
    public async Task<BookingResponse<BookingVehiculoDetailData>> GetVehiculoDetalleAsync(int vehiculoId)
    {
        var v = await _vehiculoDataService.GetByIdAsync(vehiculoId)
            ?? throw new NotFoundException($"Vehículo con ID {vehiculoId} no encontrado");

        var extrasGlobal = (await _bookingDataService.GetExtrasConPrecioAsync()).ToList();
        var montoBase = v.PrecioBaseDia;
        var impuestos = Math.Round(montoBase * TasaIva, 2);

        var dto = MapVehiculoResponse(v, montoBase, impuestos, extrasGlobal,
            null, null, incluirCategoria: true);

        return BookingResponse<BookingVehiculoDetailData>.Ok(new BookingVehiculoDetailData { Vehiculo = dto });
    }

    // =====================================================
    // Endpoint 3: Verificar disponibilidad en tiempo real
    // =====================================================
    public async Task<BookingResponse<BookingDisponibilidadCheckData>> VerificarDisponibilidadAsync(
        int vehiculoId, BookingDisponibilidadRequest request)
    {
        var v = await _vehiculoDataService.GetByIdAsync(vehiculoId)
            ?? throw new NotFoundException($"Vehículo con ID {vehiculoId} no encontrado");

        // Verificar que el vehículo esté en la localización solicitada
        var enLocalizacion = v.IdLocalizacion == request.IdLocalizacion;

        // Verificar que no haya solapamiento de reservas
        var solapado = await _reservaDataService.ExisteSolapamientoAsync(
            vehiculoId, request.FechaRecogida, request.FechaDevolucion);

        // Verificar estado operativo
        var disponible = enLocalizacion && !solapado && v.EstadoOperativo == "DISPONIBLE";

        var data = new BookingDisponibilidadCheckData
        {
            VehiculoId = v.CodigoInterno,
            Disponibilidad = new BookingDisponibilidadDto
            {
                FechaInicio = request.FechaRecogida.ToString("yyyy-MM-ddTHH:mm:ss"),
                FechaFin = request.FechaDevolucion.ToString("yyyy-MM-ddTHH:mm:ss"),
                Disponible = disponible
            }
        };

        return BookingResponse<BookingDisponibilidadCheckData>.Ok(data);
    }

    // =====================================================
    // Endpoint 4: Listar localizaciones (paginado)
    // =====================================================
    public async Task<BookingResponse<BookingLocalizacionListData>> GetLocalizacionesAsync(BookingLocalizacionesRequest request)
    {
        if (request.Page < 1) request.Page = 1;
        if (request.Limit < 1) request.Limit = 20;
        if (request.Limit > 100) request.Limit = 100;

        var todas = (await _bookingDataService.GetLocalizacionesConCiudadIdAsync()).ToList();

        // Filtro por ciudad si se proporciona
        if (request.IdCiudad.HasValue)
            todas = todas.Where(l => l.IdCiudad == request.IdCiudad.Value).ToList();

        var totalElementos = todas.Count;
        var totalPaginas = (int)Math.Ceiling((double)totalElementos / request.Limit);
        var paginadas = todas.Skip((request.Page - 1) * request.Limit).Take(request.Limit).ToList();

        var localizaciones = paginadas.Select(l => new BookingLocalizacionResponse
        {
            Id = l.IdLocalizacion,
            Codigo = l.CodigoLocalizacion,
            Nombre = l.NombreLocalizacion,
            Direccion = l.DireccionLocalizacion,
            Telefono = l.TelefonoContacto,
            Correo = l.CorreoContacto,
            HorarioAtencion = l.HorarioAtencion,
            Ciudad = new BookingCiudadDto
            {
                Id = l.IdCiudad,
                Nombre = l.NombreCiudad
            },
            _links = new Dictionary<string, LinkDto>
            {
                ["self"] = new() { Href = $"/api/v1/localizaciones/{l.IdLocalizacion}" }
            }
        }).ToList();

        var data = new BookingLocalizacionListData
        {
            Localizaciones = localizaciones,
            Paginacion = new PaginacionDto
            {
                PaginaActual = request.Page,
                TotalPaginas = totalPaginas,
                TotalElementos = totalElementos,
                ElementosPorPagina = request.Limit
            },
            _links = BuildPaginationLinks("/api/v1/localizaciones", request.Page, totalPaginas, request.Limit)
        };

        return BookingResponse<BookingLocalizacionListData>.Ok(data);
    }

    // =====================================================
    // Endpoint 5: Detalle de una localización
    // =====================================================
    public async Task<BookingResponse<BookingLocalizacionDetailData>> GetLocalizacionDetalleAsync(int localizacionId)
    {
        var l = await _bookingDataService.GetLocalizacionConCiudadIdAsync(localizacionId)
            ?? throw new NotFoundException($"Localización con ID {localizacionId} no encontrada");

        var dto = new BookingLocalizacionResponse
        {
            Id = l.IdLocalizacion,
            Codigo = l.CodigoLocalizacion,
            Nombre = l.NombreLocalizacion,
            Direccion = l.DireccionLocalizacion,
            Telefono = l.TelefonoContacto,
            Correo = l.CorreoContacto,
            HorarioAtencion = l.HorarioAtencion,
            ZonaHoraria = l.ZonaHoraria,
            Ciudad = new BookingCiudadDto
            {
                Id = l.IdCiudad,
                Nombre = l.NombreCiudad
            },
            _links = new Dictionary<string, LinkDto>
            {
                ["self"] = new() { Href = $"/api/v1/localizaciones/{l.IdLocalizacion}" }
            }
        };

        return BookingResponse<BookingLocalizacionDetailData>.Ok(
            new BookingLocalizacionDetailData { Localizacion = dto });
    }

    // =====================================================
    // Endpoint 6: Listar categorías
    // =====================================================
    public async Task<BookingResponse<BookingCategoriaListData>> GetCategoriasAsync()
    {
        var categorias = (await _catalogoDataService.GetCategoriasAsync())
            .Select(c => new BookingCategoriaResponse
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion
            }).ToList();

        return BookingResponse<BookingCategoriaListData>.Ok(
            new BookingCategoriaListData { Categorias = categorias });
    }

    // =====================================================
    // Endpoint 7: Listar extras disponibles con precio real
    // =====================================================
    public async Task<BookingResponse<BookingExtraListData>> GetExtrasAsync()
    {
        var extras = (await _bookingDataService.GetExtrasConPrecioAsync())
            .Select(e => new BookingExtraResponse
            {
                Id = e.IdExtra,
                Codigo = e.CodigoExtra,
                Nombre = e.NombreExtra,
                Descripcion = e.DescripcionExtra,
                ValorFijo = e.ValorFijo
            }).ToList();

        return BookingResponse<BookingExtraListData>.Ok(
            new BookingExtraListData { Extras = extras });
    }

    // =====================================================
    // Métodos privados de mapeo
    // =====================================================
    private static BookingVehiculoResponse MapVehiculoResponse(
        VehiculoModel v,
        decimal montoBase,
        decimal impuestos,
        List<BookingExtraModel> extras,
        DateTimeOffset? fechaInicio,
        DateTimeOffset? fechaFin,
        bool incluirCategoria)
    {
        var dto = new BookingVehiculoResponse
        {
            Id = v.CodigoInterno,
            Marca = v.Marca,
            Modelo = v.Modelo,
            MarcaModelo = $"{v.Marca} {v.Modelo}",
            Anio = v.AnioFabricacion,
            ImagenUrl = v.ImagenUrl,
            Transmision = v.TipoTransmision,
            Combustible = v.TipoCombustible,
            CapacidadPasajeros = v.CapacidadPasajeros,
            CapacidadMaletas = v.CapacidadMaletas,
            NumeroPuertas = v.NumeroPuertas,
            AireAcondicionado = v.AireAcondicionado,
            Estado = v.EstadoOperativo,
            Localizacion = new BookingLocalizacionCorta
            {
                Id = v.IdLocalizacion,
                Nombre = v.NombreLocalizacion,
                Direccion = string.Empty
            },
            Precio = new BookingPrecioDto
            {
                MontoBase = montoBase,
                Impuestos = impuestos,
                Total = montoBase + impuestos
            },
            ExtrasDisponibles = extras.Select(e => new BookingExtraCorto
            {
                Id = e.IdExtra,
                Nombre = e.NombreExtra,
                Precio = e.ValorFijo
            }).ToList(),
            _links = new Dictionary<string, LinkDto>
            {
                ["self"] = new() { Href = $"/api/v1/vehiculos/{v.CodigoInterno}" },
                ["disponibilidad"] = new() { Href = $"/api/v1/vehiculos/{v.CodigoInterno}/disponibilidad" }
            }
        };

        if (incluirCategoria)
        {
            dto.Categoria = new BookingCategoriaDto
            {
                Nombre = v.Categoria
            };
        }

        if (fechaInicio.HasValue && fechaFin.HasValue)
        {
            dto.Disponibilidad = new BookingDisponibilidadDto
            {
                FechaInicio = fechaInicio.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
                FechaFin = fechaFin.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
                Disponible = true
            };
        }

        return dto;
    }

    private static Dictionary<string, LinkDto> BuildPaginationLinks(string basePath, int page, int totalPaginas, int limit)
    {
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new() { Href = $"{basePath}?page={page}&limit={limit}" }
        };

        if (page < totalPaginas)
            links["next"] = new() { Href = $"{basePath}?page={page + 1}&limit={limit}" };

        if (page > 1)
            links["prev"] = new() { Href = $"{basePath}?page={page - 1}&limit={limit}" };

        return links;
    }
}
