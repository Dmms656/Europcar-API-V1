using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using RedCar.Reservas.Api.Contracts;
using RedCar.Reservas.Api.Messaging;
using RedCar.Reservas.DataAccess.Context;
using RedCar.Reservas.DataAccess.Entities;
using RedCar.Shared.Contracts.Common;
using RedCar.Shared.Events;
using RedCar.Shared.Events.Reservas;
using RedCar.Shared.Protos.Reservas;

namespace RedCar.Reservas.Api.Services;

/// <summary>
/// Alta y cancelación transaccional de reservas (gRPC CrearReserva / CancelarReserva).
/// </summary>
public sealed class ReservasWriteService
{
    private const decimal IvaRate = 0.12m;
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ReservasDbContext _db;
    private readonly ReservasReadService _read;
    private readonly OutboxService _outbox;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReservasWriteService> _logger;

    public ReservasWriteService(
        ReservasDbContext db,
        ReservasReadService read,
        OutboxService outbox,
        IHttpClientFactory httpFactory,
        IConfiguration configuration,
        ILogger<ReservasWriteService> logger)
    {
        _db = db;
        _read = read;
        _outbox = outbox;
        _httpFactory = httpFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<CrearReservaResponse> CrearReservaAsync(CrearReservaRequest request, CancellationToken ct) =>
        CrearReservaAsync(request, ct, correlationId: null);

    public async Task<CrearReservaResponse> CrearReservaAsync(CrearReservaRequest request, CancellationToken ct, Guid? correlationId)
    {
        ValidateCrearRequest(request);

        var fechaInicio = DateOnly.Parse(request.FechaInicio, CultureInfo.InvariantCulture);
        var fechaFin = DateOnly.Parse(request.FechaFin, CultureInfo.InvariantCulture);
        var horaInicio = TimeOnly.Parse(request.HoraInicio, CultureInfo.InvariantCulture);
        var horaFin = TimeOnly.Parse(request.HoraFin, CultureInfo.InvariantCulture);

        var fechaRecogida = new DateTimeOffset(fechaInicio.ToDateTime(horaInicio, DateTimeKind.Utc));
        var fechaDevolucion = new DateTimeOffset(fechaFin.ToDateTime(horaFin, DateTimeKind.Utc));

        if (fechaDevolucion <= fechaRecogida)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "fechaDevolucion debe ser posterior a fechaRecogida."));
        }

        var disponibilidad = await _read.VerificarDisponibilidadAsync(
            request.IdVehiculo,
            request.IdLocalizacionRecogida,
            fechaRecogida,
            fechaDevolucion,
            ct);

        if (!disponibilidad.Disponible)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "El vehiculo no esta disponible para esas fechas."));
        }

        var vehiculo = await GetVehiculoAsync(request.IdVehiculo, ct)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Vehiculo {request.IdVehiculo} no encontrado."));

        var idCliente = request.IdCliente > 0
            ? request.IdCliente
            : await ResolveClienteIdAsync(request.Cliente, ct);

        if (idCliente <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "id_cliente es obligatorio."));
        }

        var cantidadDias = Math.Max(1, (fechaFin.DayNumber - fechaInicio.DayNumber) + 1);
        var subtotalVehiculo = Math.Round(vehiculo.PrecioBaseDia * cantidadDias, 2);
        var cargoOneWay = request.IdLocalizacionRecogida != request.IdLocalizacionDevolucion ? 25m : 0m;

        var extrasLines = new List<ReservaExtraLine>();
        decimal subtotalExtras = 0m;

        if (request.Extras.Count > 0)
        {
            var extraIds = request.Extras.Select(e => e.IdExtra).Distinct().ToList();
            var catalogoExtras = await GetExtrasAsync(extraIds, ct);
            var byId = catalogoExtras.ToDictionary(e => e.IdExtra);

            foreach (var item in request.Extras)
            {
                if (item.Cantidad <= 0)
                {
                    continue;
                }

                if (!byId.TryGetValue(item.IdExtra, out var catExtra))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Extra {item.IdExtra} no encontrado."));
                }

                var lineSubtotal = Math.Round(catExtra.ValorFijo * item.Cantidad, 2);
                subtotalExtras += lineSubtotal;
                extrasLines.Add(new ReservaExtraLine
                {
                    ReservaExtraGuid = Guid.NewGuid(),
                    IdExtra = item.IdExtra,
                    Cantidad = item.Cantidad,
                    ValorUnitarioExtra = catExtra.ValorFijo,
                    SubtotalExtra = lineSubtotal,
                    EstadoReservaExtra = "ACT",
                    EsEliminado = false,
                    FechaRegistroUtc = DateTimeOffset.UtcNow,
                    CreadoPorUsuario = "BOOKING_API",
                    OrigenRegistro = "API"
                });
            }
        }

        var subtotal = subtotalVehiculo + subtotalExtras;
        var iva = Math.Round(subtotal * IvaRate, 2);
        var total = Math.Round(subtotal + iva + cargoOneWay, 2);

        var canal = NormalizeCanal(request.OrigenCanalReserva);
        var now = DateTimeOffset.UtcNow;
        var codigoReserva = await GenerarCodigoReservaAsync(ct);
        var codigoConfirmacion = $"CNF-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

        var reserva = new Reserva
        {
            ReservaGuid = Guid.NewGuid(),
            CodigoReserva = codigoReserva,
            IdCliente = idCliente,
            IdVehiculo = request.IdVehiculo,
            IdLocalizacionRecogida = request.IdLocalizacionRecogida,
            IdLocalizacionDevolucion = request.IdLocalizacionDevolucion,
            CanalReserva = canal,
            FechaHoraRecogida = fechaRecogida,
            FechaHoraDevolucion = fechaDevolucion,
            Subtotal = subtotalVehiculo,
            ValorImpuestos = iva,
            ValorExtras = subtotalExtras,
            ValorDepositoGarantia = 0m,
            CargoOneWay = cargoOneWay,
            Total = total,
            CodigoConfirmacion = codigoConfirmacion,
            EstadoReserva = "CONFIRMADA",
            RequiereHold = true,
            FechaRegistroUtc = now,
            CreadoPorUsuario = "BOOKING_API",
            OrigenRegistro = "API"
        };

        foreach (var c in request.Conductores)
        {
            if (c.IdConductor <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    $"id_conductor es obligatorio para {c.NumeroIdentificacion}."));
            }

            var idConductor = c.IdConductor;

            reserva.Conductores.Add(new ReservaConductorLink
            {
                ReservaConductorGuid = Guid.NewGuid(),
                IdConductor = idConductor,
                TipoConductor = c.EsPrincipal ? "TITULAR" : "ADICIONAL",
                EsPrincipal = c.EsPrincipal,
                CargoConductorJoven = c.EdadConductor is >= 21 and <= 24 ? 5m : 0m,
                EstadoReservaConductor = "ACT",
                EsEliminado = false,
                FechaAsignacionUtc = now,
                CreadoPorUsuario = "BOOKING_API",
                OrigenRegistro = "API"
            });
        }

        foreach (var extra in extrasLines)
        {
            reserva.Extras.Add(extra);
        }

        _db.Reservas.Add(reserva);

        var response = new CrearReservaResponse
        {
            CodigoReserva = codigoReserva,
            EstadoReserva = "CONFIRMADA",
            FechaReservaUtc = now.ToString("O", CultureInfo.InvariantCulture),
            CantidadDias = cantidadDias,
            SubtotalVehiculo = (double)subtotalVehiculo,
            SubtotalExtras = (double)subtotalExtras,
            Subtotal = (double)subtotal,
            Iva = (double)iva,
            Total = (double)total
        };

        var cid = correlationId ?? Guid.CreateVersion7();
        _outbox.Stage(RoutingKeys.ReservaCreada, cid, new ReservaCreadaPayload(
                codigoReserva, "CONFIRMADA", request.IdVehiculo, request.IdLocalizacionRecogida, idCliente,
                now.ToString("O", CultureInfo.InvariantCulture), cantidadDias,
                (double)subtotalVehiculo, (double)subtotalExtras, (double)subtotal, (double)iva, (double)total));
        _outbox.Stage(RoutingKeys.DisponibilidadInvalidada, cid,
            new DisponibilidadInvalidadaPayload(request.IdVehiculo, request.IdLocalizacionRecogida, "reserva_creada"));

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Reserva creada {Codigo} cliente={Cliente} vehiculo={Vehiculo}",
            codigoReserva, idCliente, request.IdVehiculo);

        return response;
    }

    public async Task<CancelarReservaResponse> CancelarReservaAsync(CancelarReservaRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CodigoReserva))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "codigo_reserva es obligatorio."));
        }

        var codigo = request.CodigoReserva.Trim();
        var reserva = await _db.Reservas
            .FirstOrDefaultAsync(r => r.CodigoReserva == codigo, ct);

        if (reserva is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Reserva no encontrada."));
        }

        if (reserva.EstadoReserva == "CANCELADA")
        {
            return new CancelarReservaResponse
            {
                CodigoReserva = reserva.CodigoReserva,
                EstadoReserva = reserva.EstadoReserva,
                FechaCancelacionUtc = (reserva.FechaCancelacionUtc ?? DateTimeOffset.UtcNow)
                    .ToString("O", CultureInfo.InvariantCulture)
            };
        }

        var cancelUtc = DateTimeOffset.UtcNow;
        reserva.EstadoReserva = "CANCELADA";
        reserva.FechaCancelacionUtc = cancelUtc;
        reserva.MotivoCancelacion = string.IsNullOrWhiteSpace(request.MotivoCancelacion)
            ? "Cancelada desde booking"
            : request.MotivoCancelacion.Trim();
        reserva.ModificadoPorUsuario = string.IsNullOrWhiteSpace(request.UsuarioCancelacion)
            ? "BOOKING"
            : request.UsuarioCancelacion.Trim();
        reserva.FechaModificacionUtc = cancelUtc;

        var cancelResponse = new CancelarReservaResponse
        {
            CodigoReserva = reserva.CodigoReserva,
            EstadoReserva = reserva.EstadoReserva,
            FechaCancelacionUtc = cancelUtc.ToString("O", CultureInfo.InvariantCulture)
        };

        var correlationId = Guid.CreateVersion7();
        _outbox.Stage(RoutingKeys.ReservaCancelada, correlationId, new ReservaCanceladaPayload(
            reserva.CodigoReserva, reserva.EstadoReserva, reserva.IdVehiculo,
            cancelUtc.ToString("O", CultureInfo.InvariantCulture)));
        _outbox.Stage(RoutingKeys.DisponibilidadInvalidada, correlationId,
            new DisponibilidadInvalidadaPayload(reserva.IdVehiculo, reserva.IdLocalizacionRecogida, "reserva_cancelada"));

        await _db.SaveChangesAsync(ct);

        return cancelResponse;
    }

    private static void ValidateCrearRequest(CrearReservaRequest request)
    {
        if (request.IdVehiculo <= 0 || request.IdLocalizacionRecogida <= 0 || request.IdLocalizacionDevolucion <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Ids de vehiculo/localizacion invalidos."));
        }

        if (request.Conductores.Count == 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Debe enviarse al menos un conductor."));
        }

        if (request.Conductores.Count(c => c.EsPrincipal) != 1)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Debe haber exactamente un conductor principal."));
        }

        if (request.Cliente is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "cliente es obligatorio."));
        }
    }

    private static string NormalizeCanal(string? canal)
    {
        var c = (canal ?? "BOOKING").Trim().ToUpperInvariant();
        return c switch
        {
            "WEB" or "POS" or "BOOKING" or "APP" or "BACKOFFICE" => c,
            _ => "BOOKING"
        };
    }

    private async Task<string> GenerarCodigoReservaAsync(CancellationToken ct)
    {
        for (var i = 0; i < 5; i++)
        {
            var suffix = Random.Shared.Next(1000, 9999);
            var codigo = $"RES-{DateTime.UtcNow:yyMMdd}-{suffix}";
            var exists = await _db.Reservas.AsNoTracking().AnyAsync(r => r.CodigoReserva == codigo, ct);
            if (!exists)
            {
                return codigo;
            }
        }

        return $"RES-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
    }

    private async Task<int> ResolveClienteIdAsync(ClienteDto cliente, CancellationToken ct)
    {
        var body = new
        {
            nombres = cliente.Nombres,
            apellidos = cliente.Apellidos,
            tipoIdentificacion = cliente.TipoIdentificacion,
            numeroIdentificacion = cliente.NumeroIdentificacion,
            correo = cliente.Correo,
            telefono = cliente.Telefono
        };

        var result = await PostClientesAsync<ClienteUpsertPayload>("api/v1/clientes/upsert", body, ct);
        return result?.IdCliente ?? 0;
    }

    private async Task<CatalogoVehiculoPayload?> GetVehiculoAsync(int idVehiculo, CancellationToken ct)
        => await GetDownstreamAsync<CatalogoVehiculoPayload>(
            "DownstreamCatalogo",
            "Downstream:CatalogoUrl",
            $"api/v1/vehiculos/{idVehiculo}",
            ct);

    private async Task<IReadOnlyList<CatalogoExtraPayload>> GetExtrasAsync(IReadOnlyList<int> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<CatalogoExtraPayload>();
        }

        var qs = string.Join(',', ids);
        var list = await GetDownstreamAsync<List<CatalogoExtraPayload>>(
            "DownstreamCatalogo",
            "Downstream:CatalogoUrl",
            $"api/v1/extras/by-ids?ids={qs}",
            ct);

        return list ?? new List<CatalogoExtraPayload>();
    }

    private async Task<TResponse?> PostClientesAsync<TResponse>(string relativePath, object body, CancellationToken ct)
        where TResponse : class
    {
        var baseUrl = _configuration["Downstream:ClientesUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return default;
        }

        var url = baseUrl.TrimEnd('/') + "/" + relativePath.TrimStart('/');
        try
        {
            var client = _httpFactory.CreateClient("DownstreamClientes");
            using var resp = await client.PostAsJsonAsync(url, body, Json, ct);
            if (!resp.IsSuccessStatusCode)
            {
                return default;
            }

            var envelope = await resp.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(Json, ct);
            return envelope?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fallo POST clientes {Url}", url);
            return default;
        }
    }

    private async Task<T?> GetDownstreamAsync<T>(string httpClientName, string configKeyBaseUrl, string relativePath, CancellationToken ct)
        where T : class
    {
        var baseUrl = _configuration[configKeyBaseUrl];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return default;
        }

        var url = baseUrl.TrimEnd('/') + "/" + relativePath.TrimStart('/');
        try
        {
            var client = _httpFactory.CreateClient(httpClientName);
            using var resp = await client.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
            {
                return default;
            }

            var envelope = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(Json, ct);
            return envelope?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fallo downstream GET {Url}", url);
            return default;
        }
    }

    private sealed class CatalogoVehiculoPayload
    {
        public int IdVehiculo { get; set; }
        public decimal PrecioBaseDia { get; set; }
        public int IdLocalizacion { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    private sealed class CatalogoExtraPayload
    {
        public int IdExtra { get; set; }
        public decimal ValorFijo { get; set; }
    }

    private sealed class ClienteUpsertPayload
    {
        public int IdCliente { get; set; }
    }

}
