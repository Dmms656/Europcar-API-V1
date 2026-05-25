using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RedCar.Reservas.Api.Contracts;
using RedCar.Reservas.DataAccess.Context;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Reservas.Api.Services;

/// <summary>
/// Lecturas de reserva/factura/disponibilidad con datos enriquecidos desde otros MS (HTTP opcional).
/// </summary>
public sealed class ReservasReadService
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ReservasDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ReservasReadService> _logger;

    public ReservasReadService(
        ReservasDbContext db,
        IHttpClientFactory httpFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ReservasReadService> logger)
    {
        _db = db;
        _httpFactory = httpFactory;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<DisponibilidadDto> VerificarDisponibilidadAsync(
        int idVehiculo,
        int idLocalizacion,
        DateTimeOffset fechaRecogida,
        DateTimeOffset fechaDevolucion,
        CancellationToken ct)
    {
        var disponible = true;

        if (fechaDevolucion <= fechaRecogida)
        {
            disponible = false;
        }
        else
        {
            var conflicto = await _db.Reservas
                .AsNoTracking()
                .Where(r => r.IdVehiculo == idVehiculo)
                .Where(r => r.EstadoReserva != "CANCELADA" && r.EstadoReserva != "FINALIZADA" && r.EstadoReserva != "NO_SHOW")
                .Where(r => r.FechaHoraRecogida < fechaDevolucion && r.FechaHoraDevolucion > fechaRecogida)
                .AnyAsync(ct);

            if (conflicto)
            {
                disponible = false;
            }
        }

        if (disponible)
        {
            var catalogoUrl = _configuration["Downstream:CatalogoUrl"];
            if (!string.IsNullOrWhiteSpace(catalogoUrl))
            {
                var v = await GetCatalogoVehiculoAsync(idVehiculo, ct);
                if (v is null || v.IdLocalizacion != idLocalizacion || !string.Equals(v.Estado, "DISPONIBLE", StringComparison.OrdinalIgnoreCase))
                {
                    disponible = false;
                }
            }
        }

        return new DisponibilidadDto
        {
            IdVehiculo = idVehiculo,
            IdLocalizacion = idLocalizacion,
            FechaRecogida = fechaRecogida,
            FechaDevolucion = fechaDevolucion,
            Disponible = disponible
        };
    }

    public async Task<IReadOnlyList<ClienteReservaListItemDto>> ListByClienteAsync(int idCliente, CancellationToken ct)
    {
        var rows = await _db.Reservas
            .AsNoTracking()
            .Include(x => x.Extras)
            .Where(r => r.IdCliente == idCliente)
            .OrderByDescending(r => r.FechaRegistroUtc)
            .ToListAsync(ct);

        if (rows.Count == 0)
        {
            return Array.Empty<ClienteReservaListItemDto>();
        }

        var vehiculoCache = new Dictionary<int, CatalogoVehiculoPayload?>();
        var cliente = await GetClienteAsync(idCliente, ct);
        var nombreCliente = cliente is null
            ? null
            : $"{cliente.Nombres} {cliente.Apellidos}".Trim();

        var result = new List<ClienteReservaListItemDto>(rows.Count);
        foreach (var r in rows)
        {
            if (!vehiculoCache.TryGetValue(r.IdVehiculo, out var vehiculo))
            {
                vehiculo = await GetCatalogoVehiculoAsync(r.IdVehiculo, ct);
                vehiculoCache[r.IdVehiculo] = vehiculo;
            }

            var extrasActivos = r.Extras.Where(e => !e.EsEliminado && e.EstadoReservaExtra == "ACT").ToList();
            var extraIds = extrasActivos.Select(e => e.IdExtra).Distinct().ToList();
            var extraNombres = await GetExtraNombresAsync(extraIds, ct);

            result.Add(new ClienteReservaListItemDto
            {
                IdReserva = r.IdReserva,
                ReservaGuid = r.ReservaGuid,
                CodigoReserva = r.CodigoReserva,
                CodigoConfirmacion = string.IsNullOrWhiteSpace(r.CodigoConfirmacion) ? r.CodigoReserva : r.CodigoConfirmacion,
                EstadoReserva = r.EstadoReserva,
                IdCliente = r.IdCliente,
                IdVehiculo = r.IdVehiculo,
                IdLocalizacionRecogida = r.IdLocalizacionRecogida,
                IdLocalizacionDevolucion = r.IdLocalizacionDevolucion,
                CanalReserva = r.CanalReserva,
                FechaHoraRecogida = r.FechaHoraRecogida,
                FechaHoraDevolucion = r.FechaHoraDevolucion,
                Subtotal = r.Subtotal,
                ValorImpuestos = r.ValorImpuestos,
                ValorExtras = r.ValorExtras,
                CargoOneWay = r.CargoOneWay,
                Total = r.Total,
                NombreCliente = nombreCliente,
                PlacaVehiculo = null,
                DescripcionVehiculo = vehiculo is null
                    ? $"Vehículo {r.IdVehiculo}"
                    : $"{vehiculo.Marca} {vehiculo.Modelo}".Trim(),
                Extras = extrasActivos.Select(e => new ReservaExtraListItemDto
                {
                    IdReservaExtra = e.IdReservaExtra,
                    IdExtra = e.IdExtra,
                    CodigoExtra = $"EXT-{e.IdExtra}",
                    NombreExtra = extraNombres.GetValueOrDefault(e.IdExtra, $"Extra {e.IdExtra}"),
                    Cantidad = e.Cantidad,
                    ValorUnitario = e.ValorUnitarioExtra,
                    Subtotal = e.SubtotalExtra
                }).ToList()
            });
        }

        return result;
    }

    public async Task<string?> GetCodigoReservaByIdAsync(int idReserva, CancellationToken ct)
    {
        return await _db.Reservas
            .AsNoTracking()
            .Where(r => r.IdReserva == idReserva)
            .Select(r => r.CodigoReserva)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ReservaDto?> GetReservaAsync(string codigoReserva, CancellationToken ct)
    {
        var r = await _db.Reservas
            .AsNoTracking()
            .Include(x => x.Conductores)
            .Include(x => x.Extras)
            .FirstOrDefaultAsync(x => x.CodigoReserva == codigoReserva, ct);

        if (r is null)
        {
            return null;
        }

        var vehiculo = await GetCatalogoVehiculoAsync(r.IdVehiculo, ct);
        var locRec = await GetLocalizacionAsync(r.IdLocalizacionRecogida, ct);
        var locDev = await GetLocalizacionAsync(r.IdLocalizacionDevolucion, ct);
        var cliente = await GetClienteAsync(r.IdCliente, ct);

        var conductores = new List<ReservaConductorDto>();
        foreach (var rc in r.Conductores.Where(c => !c.EsEliminado && c.EstadoReservaConductor == "ACT").OrderByDescending(c => c.EsPrincipal))
        {
            var cond = await GetConductorAsync(rc.IdConductor, ct);
            if (cond is null)
            {
                continue;
            }

            conductores.Add(new ReservaConductorDto
            {
                Nombres = cond.Nombres,
                Apellidos = cond.Apellidos,
                TipoIdentificacion = cond.TipoIdentificacion,
                NumeroIdentificacion = cond.NumeroIdentificacion,
                EdadConductor = cond.EdadConductor,
                EsPrincipal = rc.EsPrincipal
            });
        }

        var extrasActivos = r.Extras.Where(e => !e.EsEliminado && e.EstadoReservaExtra == "ACT").ToList();
        var extraIds = extrasActivos.Select(e => e.IdExtra).Distinct().ToList();
        var extraNombres = await GetExtraNombresAsync(extraIds, ct);

        var extrasDto = extrasActivos.Select(e => new ReservaExtraDto
        {
            IdExtra = e.IdExtra,
            Nombre = extraNombres.GetValueOrDefault(e.IdExtra, $"Extra {e.IdExtra}"),
            Cantidad = e.Cantidad,
            ValorUnitario = e.ValorUnitarioExtra,
            Subtotal = e.SubtotalExtra
        }).ToList();

        var fi = r.FechaHoraRecogida;
        var ff = r.FechaHoraDevolucion;
        var fechaInicio = DateOnly.FromDateTime(fi.UtcDateTime);
        var fechaFin = DateOnly.FromDateTime(ff.UtcDateTime);
        var horaInicio = TimeOnly.FromTimeSpan(fi.UtcDateTime.TimeOfDay);
        var horaFin = TimeOnly.FromTimeSpan(ff.UtcDateTime.TimeOfDay);
        var dias = Math.Max(1, (fechaFin.DayNumber - fechaInicio.DayNumber) + 1);

        return new ReservaDto
        {
            CodigoReserva = r.CodigoReserva,
            EstadoReserva = r.EstadoReserva,
            OrigenCanalReserva = r.CanalReserva,
            FechaReservaUtc = r.FechaRegistroUtc,
            FechaConfirmacionUtc = r.EstadoReserva == "CONFIRMADA" ? r.FechaRegistroUtc : null,
            FechaCancelacionUtc = r.FechaCancelacionUtc,
            MotivoCancelacion = r.MotivoCancelacion,
            Observaciones = null,
            Vehiculo = new ReservaVehiculoDto
            {
                IdVehiculo = r.IdVehiculo,
                CodigoInterno = vehiculo?.CodigoInterno ?? string.Empty,
                Marca = vehiculo?.Marca ?? string.Empty,
                Modelo = vehiculo?.Modelo ?? string.Empty
            },
            LocalizacionRecogida = new ReservaLocalizacionDto
            {
                IdLocalizacion = r.IdLocalizacionRecogida,
                Nombre = locRec ?? string.Empty
            },
            LocalizacionDevolucion = new ReservaLocalizacionDto
            {
                IdLocalizacion = r.IdLocalizacionDevolucion,
                Nombre = locDev ?? string.Empty
            },
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            CantidadDias = dias,
            Cliente = cliente ?? new ReservaClienteDto
            {
                Nombres = $"Cliente {r.IdCliente}",
                Apellidos = string.Empty,
                TipoIdentificacion = "CEDULA",
                NumeroIdentificacion = string.Empty,
                Correo = string.Empty,
                Telefono = string.Empty
            },
            Conductores = conductores,
            Extras = extrasDto,
            SubtotalVehiculo = r.Subtotal,
            SubtotalExtras = r.ValorExtras,
            Subtotal = r.Subtotal + r.ValorExtras,
            Iva = r.ValorImpuestos,
            Total = r.Total
        };
    }

    public async Task<FacturaDto?> GetFacturaAsync(string codigoReserva, CancellationToken ct)
    {
        var f = await _db.Facturas
            .AsNoTracking()
            .Include(x => x.Reserva)
            .Where(x => !x.EsEliminado && x.EstadoFactura != "ANULADA")
            .FirstOrDefaultAsync(x => x.Reserva != null && x.Reserva.CodigoReserva == codigoReserva, ct);

        if (f is null)
        {
            return null;
        }

        return new FacturaDto
        {
            NumeroFactura = f.NumeroFactura,
            CodigoReserva = codigoReserva,
            FechaFacturaUtc = f.FechaEmision,
            Subtotal = f.Subtotal,
            Iva = f.ValorIva,
            Total = f.Total,
            Moneda = "USD",
            UrlPdf = null
        };
    }

    private async Task<CatalogoVehiculoPayload?> GetCatalogoVehiculoAsync(int idVehiculo, CancellationToken ct)
        => await GetDownstreamAsync<CatalogoVehiculoPayload>("DownstreamCatalogo", "Downstream:CatalogoUrl", $"api/v1/vehiculos/{idVehiculo}", ct);

    private async Task<string?> GetLocalizacionAsync(int idLocalizacion, CancellationToken ct)
    {
        var dto = await GetDownstreamAsync<LocalizacionPayload>("DownstreamLocalizaciones", "Downstream:LocalizacionesUrl", $"api/v1/localizaciones/{idLocalizacion}", ct);
        return dto?.Nombre;
    }

    private async Task<ReservaClienteDto?> GetClienteAsync(int idCliente, CancellationToken ct)
    {
        var c = await GetDownstreamAsync<ClienteDetallePayload>("DownstreamClientes", "Downstream:ClientesUrl", $"api/v1/clientes/{idCliente}", ct);
        if (c is null) return null;
        return new ReservaClienteDto
        {
            Nombres = c.Nombres,
            Apellidos = c.Apellidos,
            TipoIdentificacion = c.TipoIdentificacion,
            NumeroIdentificacion = c.NumeroIdentificacion,
            Correo = c.Correo,
            Telefono = c.Telefono
        };
    }

    private async Task<ConductorDetallePayload?> GetConductorAsync(int idConductor, CancellationToken ct)
        => await GetDownstreamAsync<ConductorDetallePayload>("DownstreamClientes", "Downstream:ClientesUrl", $"api/v1/conductores/{idConductor}", ct);

    private async Task<IReadOnlyDictionary<int, string>> GetExtraNombresAsync(IReadOnlyList<int> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return new Dictionary<int, string>();
        }

        var qs = string.Join(',', ids);
        var list = await GetDownstreamAsync<List<ExtraNombrePayload>>("DownstreamCatalogo", "Downstream:CatalogoUrl", $"api/v1/extras/by-ids?ids={qs}", ct);
        if (list is null)
        {
            return new Dictionary<int, string>();
        }

        return list.ToDictionary(x => x.IdExtra, x => x.Nombre);
    }

    private async Task<T?> GetDownstreamAsync<T>(string httpClientName, string configKeyBaseUrl, string relativePath, CancellationToken ct)
        where T : class
    {
        var baseUrl = _configuration[configKeyBaseUrl];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return default;
        }

        var fullUrl = baseUrl.TrimEnd('/') + "/" + relativePath.TrimStart('/');

        try
        {
            var client = _httpFactory.CreateClient(httpClientName);
            using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            var auth = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(auth))
            {
                req.Headers.TryAddWithoutValidation("Authorization", auth);
            }

            using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!resp.IsSuccessStatusCode)
            {
                return default;
            }

            var envelope = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(Json, ct);
            return envelope?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fallo downstream GET {Url}", fullUrl);
            return default;
        }
    }

    private sealed class CatalogoVehiculoPayload
    {
        public int IdVehiculo { get; set; }
        public string CodigoInterno { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int IdLocalizacion { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    private sealed class LocalizacionPayload
    {
        public string Nombre { get; set; } = string.Empty;
    }

    private sealed class ClienteDetallePayload
    {
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }

    private sealed class ConductorDetallePayload
    {
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public int EdadConductor { get; set; }
    }

    private sealed class ExtraNombrePayload
    {
        public int IdExtra { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
