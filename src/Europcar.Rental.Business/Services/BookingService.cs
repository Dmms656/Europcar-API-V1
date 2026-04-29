using Europcar.Rental.Business.DTOs.Request.Booking;
using Europcar.Rental.Business.DTOs.Request.Reservas;
using Europcar.Rental.Business.DTOs.Response.Booking;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.Business.Services;

/// <summary>
/// Implementación del servicio de Booking / OTA.
/// Orquesta las consultas de disponibilidad, vehículos, localizaciones,
/// categorías, extras y reservas según el contrato de API externo.
/// Tasa de impuesto IVA fija: 15 % (regla de negocio Ecuador).
/// </summary>
public class BookingService : IBookingService
{
    private readonly IVehiculoDataService _vehiculoDataService;
    private readonly IReservaDataService _reservaDataService;
    private readonly IBookingDataService _bookingDataService;
    private readonly ICatalogoDataService _catalogoDataService;
    private readonly IClienteDataService _clienteDataService;
    private readonly IConductorDataService _conductorDataService;
    private readonly IReservaService _reservaService;

    private const decimal TasaIva = 0.15m;

    public BookingService(
        IVehiculoDataService vehiculoDataService,
        IReservaDataService reservaDataService,
        IBookingDataService bookingDataService,
        ICatalogoDataService catalogoDataService,
        IClienteDataService clienteDataService,
        IConductorDataService conductorDataService,
        IReservaService reservaService)
    {
        _vehiculoDataService = vehiculoDataService;
        _reservaDataService = reservaDataService;
        _bookingDataService = bookingDataService;
        _catalogoDataService = catalogoDataService;
        _clienteDataService = clienteDataService;
        _conductorDataService = conductorDataService;
        _reservaService = reservaService;
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

        // Filtro por marca
        if (request.IdMarca.HasValue)
            lista = lista.Where(v => v.IdMarca == request.IdMarca.Value).ToList();

        // Excluir vehículos con reservas solapadas en las fechas solicitadas
        // Skip overlap check if no dates provided (e.g. homepage featured vehicles)
        var hasFechas = request.FechaRecogida != default && request.FechaDevolucion != default
                        && request.FechaDevolucion > request.FechaRecogida;
        var disponibles = new List<VehiculoModel>();
        foreach (var v in lista)
        {
            if (hasFechas)
            {
                var solapado = await _reservaDataService.ExisteSolapamientoAsync(
                    v.IdVehiculo, request.FechaRecogida, request.FechaDevolucion);
                if (!solapado)
                    disponibles.Add(v);
            }
            else
            {
                disponibles.Add(v);
            }
        }

        // Calcular días de alquiler (mínimo 1)
        var dias = hasFechas
            ? Math.Max(1, (int)Math.Ceiling((request.FechaDevolucion - request.FechaRecogida).TotalDays))
            : 1;

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
    public async Task<BookingResponse<BookingVehiculoDetailData>> GetVehiculoDetalleAsync(string vehiculoId)
    {
        var v = await ResolverVehiculoAsync(vehiculoId);

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
        string vehiculoId, BookingDisponibilidadRequest request)
    {
        var v = await ResolverVehiculoAsync(vehiculoId);

        // Verificar que el vehículo esté en la localización solicitada
        var enLocalizacion = v.IdLocalizacion == request.IdLocalizacion;

        // Verificar que no haya solapamiento de reservas
        var solapado = await _reservaDataService.ExisteSolapamientoAsync(
            v.IdVehiculo, request.FechaRecogida, request.FechaDevolucion);

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

    /// <summary>
    /// Resuelve un vehículo por su identificador público (CodigoInterno) o, como respaldo,
    /// por su ID numérico cuando el identificador venga como entero. Lanza NotFoundException
    /// si no se halla en ninguna de las dos formas.
    /// </summary>
    private async Task<VehiculoModel> ResolverVehiculoAsync(string vehiculoId)
    {
        if (string.IsNullOrWhiteSpace(vehiculoId))
            throw new NotFoundException("El identificador del vehículo es requerido");

        var porCodigo = await _vehiculoDataService.GetByCodigoInternoAsync(vehiculoId);
        if (porCodigo != null) return porCodigo;

        if (int.TryParse(vehiculoId, out var idInt))
        {
            var porId = await _vehiculoDataService.GetByIdAsync(idInt);
            if (porId != null) return porId;
        }

        throw new NotFoundException($"Vehículo con ID '{vehiculoId}' no encontrado");
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

    public async Task<BookingResponse<BookingCiudadListData>> GetCiudadesAsync()
    {
        var ciudades = (await _catalogoDataService.GetCiudadesAsync())
            .Select(c => new BookingCiudadResponse
            {
                IdCiudad = c.IdCiudad,
                IdPais = c.IdPais,
                NombreCiudad = c.NombreCiudad,
                NombrePais = c.NombrePais ?? string.Empty
            })
            .OrderBy(c => c.NombrePais)
            .ThenBy(c => c.NombreCiudad)
            .ToList();

        return BookingResponse<BookingCiudadListData>.Ok(
            new BookingCiudadListData { Ciudades = ciudades });
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
    // Endpoint 8: Crear reserva (POST /api/v1/reservas)
    // =====================================================
    public async Task<BookingResponse<BookingCrearReservaData>> CrearReservaAsync(BookingCrearReservaRequest request)
    {
        if (request == null) throw new BusinessException("El cuerpo de la solicitud es requerido");
        if (request.Cliente == null) throw new BusinessException("El cliente es requerido");
        if (request.ConductorPrincipal == null) throw new BusinessException("El conductor principal es requerido");
        if (request.FechaFin <= request.FechaInicio)
            throw new BusinessException("La fecha de fin debe ser posterior a la fecha de inicio");

        // 1) Resolver el vehículo por identificador público y obtener su ID interno
        var vehiculo = await ResolverVehiculoAsync(request.IdVehiculo);

        // 2) Obtener o crear el cliente por número de identificación
        var cliente = await ObtenerOCrearClienteAsync(request.Cliente);

        // 3) Crear/asociar conductores (principal y secundario opcional)
        var conductorPrincipal = await ObtenerOCrearConductorAsync(request.ConductorPrincipal, cliente);
        ConductorModel? conductorSecundario = null;
        if (request.ConductorSecundario != null && !string.IsNullOrWhiteSpace(request.ConductorSecundario.NumeroIdentificacion))
        {
            conductorSecundario = await ObtenerOCrearConductorAsync(request.ConductorSecundario, cliente);
        }

        // 4) Construir el CrearReservaRequest interno reutilizando la lógica existente
        var horaIni = request.HoraInicio ?? new TimeOnly(9, 0);
        var horaFin = request.HoraFin ?? new TimeOnly(9, 0);
        var fechaRecogida = new DateTimeOffset(request.FechaInicio.ToDateTime(horaIni), TimeSpan.Zero);
        var fechaDevolucion = new DateTimeOffset(request.FechaFin.ToDateTime(horaFin), TimeSpan.Zero);

        var conductoresInternos = new List<ReservaConductorItemRequest>
        {
            new() { IdConductor = conductorPrincipal.IdConductor, EsPrincipal = true }
        };
        if (conductorSecundario != null)
        {
            conductoresInternos.Add(new ReservaConductorItemRequest
            {
                IdConductor = conductorSecundario.IdConductor,
                EsPrincipal = false
            });
        }

        var canalReserva = string.IsNullOrWhiteSpace(request.OrigenCanalReserva)
            ? "BOOKING"
            : request.OrigenCanalReserva.Trim().ToUpperInvariant();
        var extrasRequest = request.Extras ?? new List<BookingReservaExtraItem>();

        var crearInterno = new CrearReservaRequest
        {
            IdCliente = cliente.IdCliente,
            IdVehiculo = vehiculo.IdVehiculo,
            IdLocalizacionRecogida = request.IdLocalizacionRecogida,
            IdLocalizacionDevolucion = request.IdLocalizacionEntrega,
            CanalReserva = canalReserva,
            FechaHoraRecogida = fechaRecogida,
            FechaHoraDevolucion = fechaDevolucion,
            Conductores = conductoresInternos,
            Extras = extrasRequest.Select(e => new ReservaExtraItemRequest
            {
                IdExtra = e.IdExtra,
                Cantidad = e.Cantidad
            }).ToList()
        };

        var creada = await _reservaService.CreateAsync(crearInterno);

        var dias = Math.Max(1, (int)Math.Ceiling((fechaDevolucion - fechaRecogida).TotalDays));
        var data = new BookingCrearReservaData
        {
            CodigoReserva = creada.CodigoReserva,
            EstadoReserva = creada.EstadoReserva,
            FechaReservaUtc = DateTimeOffset.UtcNow,
            Vehiculo = new BookingVehiculoCorto
            {
                Id = vehiculo.CodigoInterno,
                MarcaModelo = $"{vehiculo.Marca} {vehiculo.Modelo}".Trim()
            },
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            CantidadDias = dias,
            Subtotal = creada.Subtotal,
            Iva = creada.ValorImpuestos,
            Total = creada.Total,
            _links = new Dictionary<string, LinkDto>
            {
                ["self"] = new() { Href = $"/api/v1/reservas/{creada.CodigoReserva}" },
                ["factura"] = new() { Href = $"/api/v1/reservas/{creada.CodigoReserva}/factura" },
                ["cancelar"] = new() { Href = $"/api/v1/reservas/{creada.CodigoReserva}/cancelar" }
            }
        };

        return BookingResponse<BookingCrearReservaData>.Created(data, "Reserva creada exitosamente");
    }

    // =====================================================
    // Endpoint 9: Detalle de reserva por código (GET)
    // =====================================================
    public async Task<BookingResponse<BookingReservaDetailData>> GetReservaByCodigoAsync(string codigoReserva)
    {
        if (string.IsNullOrWhiteSpace(codigoReserva))
            throw new BusinessException("El código de reserva es requerido");

        var reserva = await _reservaDataService.GetByCodigoAsync(codigoReserva)
            ?? throw new NotFoundException($"Reserva con código {codigoReserva} no encontrada");

        var locRec = await _bookingDataService.GetLocalizacionConCiudadIdAsync(reserva.IdLocalizacionRecogida);
        var locDev = await _bookingDataService.GetLocalizacionConCiudadIdAsync(reserva.IdLocalizacionDevolucion);
        var vehiculo = await _vehiculoDataService.GetByIdAsync(reserva.IdVehiculo);

        var fechaInicio = DateOnly.FromDateTime(reserva.FechaHoraRecogida.UtcDateTime);
        var fechaFin = DateOnly.FromDateTime(reserva.FechaHoraDevolucion.UtcDateTime);
        var dias = Math.Max(1, (int)Math.Ceiling((reserva.FechaHoraDevolucion - reserva.FechaHoraRecogida).TotalDays));

        var data = new BookingReservaDetailData
        {
            CodigoReserva = reserva.CodigoReserva,
            EstadoReserva = reserva.EstadoReserva,
            OrigenCanal = reserva.CanalReserva,
            FechaReservaUtc = reserva.FechaHoraRecogida,
            Vehiculo = new BookingVehiculoCorto
            {
                Id = vehiculo?.CodigoInterno ?? string.Empty,
                MarcaModelo = vehiculo != null
                    ? $"{vehiculo.Marca} {vehiculo.Modelo}".Trim()
                    : (reserva.DescripcionVehiculo ?? string.Empty)
            },
            LocalizacionRecogida = new BookingLocalizacionMini
            {
                Id = reserva.IdLocalizacionRecogida,
                Nombre = locRec?.NombreLocalizacion ?? string.Empty
            },
            LocalizacionEntrega = new BookingLocalizacionMini
            {
                Id = reserva.IdLocalizacionDevolucion,
                Nombre = locDev?.NombreLocalizacion ?? string.Empty
            },
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            CantidadDias = dias,
            Subtotal = reserva.Subtotal,
            Iva = reserva.ValorImpuestos,
            Total = reserva.Total,
            Extras = reserva.Extras.Select(e => new BookingReservaExtraDto
            {
                Id = e.IdExtra,
                Nombre = e.NombreExtra,
                Cantidad = e.Cantidad,
                ValorUnitario = e.ValorUnitario,
                Subtotal = e.Subtotal
            }).ToList(),
            _links = new Dictionary<string, LinkDto>
            {
                ["self"] = new() { Href = $"/api/v1/reservas/{reserva.CodigoReserva}" },
                ["factura"] = new() { Href = $"/api/v1/reservas/{reserva.CodigoReserva}/factura" },
                ["cancelar"] = new() { Href = $"/api/v1/reservas/{reserva.CodigoReserva}/cancelar" }
            }
        };

        return BookingResponse<BookingReservaDetailData>.Ok(data);
    }

    // =====================================================
    // Endpoint 10: Cancelar reserva (PATCH)
    // =====================================================
    public async Task<BookingResponse<BookingCancelarReservaData>> CancelarReservaAsync(
        string codigoReserva, BookingCancelarReservaRequest request, string usuario)
    {
        if (string.IsNullOrWhiteSpace(codigoReserva))
            throw new BusinessException("El código de reserva es requerido");
        if (request == null || string.IsNullOrWhiteSpace(request.MotivoCancelacion))
            throw new BusinessException("El motivo de cancelación es requerido");

        var reserva = await _reservaDataService.GetByCodigoAsync(codigoReserva)
            ?? throw new NotFoundException($"Reserva con código {codigoReserva} no encontrada");

        var actualizada = await _reservaService.CancelarAsync(reserva.IdReserva, request.MotivoCancelacion, usuario);

        var data = new BookingCancelarReservaData
        {
            CodigoReserva = actualizada.CodigoReserva,
            EstadoReserva = actualizada.EstadoReserva,
            FechaCancelacionUtc = DateTimeOffset.UtcNow,
            MotivoCancelacion = request.MotivoCancelacion
        };

        return BookingResponse<BookingCancelarReservaData>.Ok(data, "Reserva cancelada exitosamente");
    }

    // =====================================================
    // Endpoint 11: Factura asociada a la reserva
    // =====================================================
    public async Task<BookingResponse<BookingFacturaData>> GetFacturaPorReservaAsync(string codigoReserva)
    {
        if (string.IsNullOrWhiteSpace(codigoReserva))
            throw new BusinessException("El código de reserva es requerido");

        // Verificar que la reserva exista
        _ = await _reservaDataService.GetByCodigoAsync(codigoReserva)
            ?? throw new NotFoundException($"Reserva con código {codigoReserva} no encontrada");

        var factura = await _bookingDataService.GetFacturaByReservaCodigoAsync(codigoReserva)
            ?? throw new NotFoundException($"La reserva {codigoReserva} aún no tiene factura emitida");

        var data = new BookingFacturaData
        {
            Factura = new BookingFacturaDto
            {
                NumeroFactura = factura.NumeroFactura,
                CodigoReserva = factura.CodigoReserva,
                EstadoFactura = factura.EstadoFactura,
                FechaEmision = factura.FechaEmision,
                Cliente = new BookingClienteFacturaDto
                {
                    Nombres = factura.ClienteNombres,
                    Apellidos = factura.ClienteApellidos,
                    Identificacion = factura.ClienteIdentificacion
                },
                Subtotal = factura.Subtotal,
                Iva = factura.ValorIva,
                Total = factura.Total
            }
        };

        return BookingResponse<BookingFacturaData>.Ok(data);
    }

    // =====================================================
    // Helpers privados (cliente / conductor)
    // =====================================================
    private async Task<ClienteModel> ObtenerOCrearClienteAsync(BookingClienteData data)
    {
        if (string.IsNullOrWhiteSpace(data.NumeroIdentificacion))
            throw new BusinessException("El número de identificación del cliente es requerido");

        var existente = await _clienteDataService.GetByIdentificacionAsync(data.NumeroIdentificacion.Trim());
        if (existente != null) return existente;

        var nuevo = new ClienteModel
        {
            CodigoCliente = $"CLT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            TipoIdentificacion = string.IsNullOrWhiteSpace(data.TipoIdentificacion) ? "CED" : data.TipoIdentificacion.Trim(),
            NumeroIdentificacion = data.NumeroIdentificacion.Trim(),
            Nombre1 = data.Nombres?.Trim() ?? string.Empty,
            Apellido1 = data.Apellidos?.Trim() ?? string.Empty,
            Telefono = data.Telefono?.Trim() ?? string.Empty,
            Correo = data.Correo?.Trim() ?? string.Empty,
            FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-25))
        };

        return await _clienteDataService.CreateAsync(nuevo);
    }

    private async Task<ConductorModel> ObtenerOCrearConductorAsync(BookingConductorData data, ClienteModel cliente)
    {
        if (string.IsNullOrWhiteSpace(data.NumeroIdentificacion))
            throw new BusinessException("El número de identificación del conductor es requerido");
        if (string.IsNullOrWhiteSpace(data.NumeroLicencia))
            throw new BusinessException("El número de licencia del conductor es requerido");

        var conductoresCliente = await _conductorDataService.GetByClienteIdAsync(cliente.IdCliente);
        var existente = conductoresCliente.FirstOrDefault(c =>
            c.NumeroIdentificacion == data.NumeroIdentificacion.Trim());
        if (existente != null) return existente;

        var nuevo = new ConductorModel
        {
            CodigoConductor = $"CDT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            IdCliente = cliente.IdCliente,
            TipoIdentificacion = string.IsNullOrWhiteSpace(data.TipoIdentificacion) ? "CED" : data.TipoIdentificacion.Trim(),
            NumeroIdentificacion = data.NumeroIdentificacion.Trim(),
            Nombre1 = data.Nombres?.Trim() ?? string.Empty,
            Apellido1 = data.Apellidos?.Trim() ?? string.Empty,
            NumeroLicencia = data.NumeroLicencia.Trim(),
            FechaVencimientoLicencia = data.FechaVencimientoLicencia
                ?? DateOnly.FromDateTime(DateTime.Today.AddYears(5)),
            EdadConductor = data.EdadConductor > 0 ? data.EdadConductor : (short)25,
            Telefono = data.Telefono?.Trim() ?? string.Empty,
            Correo = data.Correo?.Trim() ?? string.Empty
        };

        return await _conductorDataService.CreateAsync(nuevo);
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
