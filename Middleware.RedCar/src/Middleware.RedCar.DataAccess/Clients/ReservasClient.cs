using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Middleware.RedCar.DataAccess.Clients.Interfaces;

namespace Middleware.RedCar.DataAccess.Clients;

public sealed class ReservasClient : HttpClientBase, IReservasClient
{
    public ReservasClient(HttpClient http, ILogger<ReservasClient> logger) : base(http, logger) { }

    public async Task<DisponibilidadDto?> VerificarDisponibilidadAsync(int idVehiculo, int idLocalizacion, DateTimeOffset fechaRecogida, DateTimeOffset fechaDevolucion, CancellationToken ct = default)
    {
        var qs = $"idVehiculo={idVehiculo}&idLocalizacion={idLocalizacion}"
            + $"&fechaRecogida={Uri.EscapeDataString(fechaRecogida.ToString("O"))}"
            + $"&fechaDevolucion={Uri.EscapeDataString(fechaDevolucion.ToString("O"))}";

        var envelope = await GetAsync<MsApiEnvelope<DisponibilidadDto>>(
            $"/api/v1/reservas/disponibilidad?{qs}", ct);
        return envelope?.Data;
    }

    public async Task<ReservaDto?> GetReservaAsync(string codigoReserva, CancellationToken ct = default)
    {
        var envelope = await GetAsync<MsApiEnvelope<ReservaDto>>(
            $"/api/v1/reservas/{Uri.EscapeDataString(codigoReserva)}", ct);
        return envelope?.Data;
    }

    public async Task<FacturaDto?> GetFacturaAsync(string codigoReserva, CancellationToken ct = default)
    {
        var envelope = await GetAsync<MsApiEnvelope<FacturaDto>>(
            $"/api/v1/reservas/{Uri.EscapeDataString(codigoReserva)}/factura", ct);
        return envelope?.Data;
    }

    public async Task<CrearReservaWriteResult> CrearReservaAsync(CrearReservaWriteRequest request, CancellationToken ct = default)
    {
        var body = new
        {
            idVehiculo = request.IdVehiculo,
            idLocalizacionRecogida = request.IdLocalizacionRecogida,
            idLocalizacionDevolucion = request.IdLocalizacionDevolucion,
            fechaInicio = request.FechaInicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            fechaFin = request.FechaFin.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            horaInicio = request.HoraInicio.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            horaFin = request.HoraFin.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            observaciones = request.Observaciones,
            origenCanalReserva = request.OrigenCanalReserva,
            idCliente = request.IdCliente,
            cliente = new
            {
                nombres = request.Cliente.Nombres,
                apellidos = request.Cliente.Apellidos,
                tipoIdentificacion = request.Cliente.TipoIdentificacion,
                numeroIdentificacion = request.Cliente.NumeroIdentificacion,
                correo = request.Cliente.Correo,
                telefono = request.Cliente.Telefono
            },
            conductores = request.Conductores.Select(c => new
            {
                idConductor = c.IdConductor,
                nombres = c.Nombres,
                apellidos = c.Apellidos,
                tipoIdentificacion = c.TipoIdentificacion,
                numeroIdentificacion = c.NumeroIdentificacion,
                fechaVencimientoLicencia = c.FechaVencimientoLicencia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                edadConductor = c.EdadConductor,
                correo = c.Correo,
                telefono = c.Telefono,
                esPrincipal = c.EsPrincipal
            }),
            extras = request.Extras.Select(e => new { idExtra = e.IdExtra, cantidad = e.Cantidad })
        };

        var data = await PostEnvelopeAsync<CrearReservaRestPayload>("/api/v1/reservas", body, ct);
        return new CrearReservaWriteResult(
            data.CodigoReserva,
            data.EstadoReserva,
            data.FechaReservaUtc,
            data.CantidadDias,
            data.SubtotalVehiculo,
            data.SubtotalExtras,
            data.Subtotal,
            data.Iva,
            data.Total);
    }

    public async Task<CancelarReservaWriteResult> CancelarReservaAsync(
        string codigoReserva, string motivo, string usuario, CancellationToken ct = default)
    {
        var body = new { motivoCancelacion = motivo, usuarioCancelacion = usuario };
        var path = $"/api/v1/reservas/{Uri.EscapeDataString(codigoReserva)}/cancelar";
        var data = await PatchEnvelopeAsync<CancelarReservaRestPayload>(path, body, ct);
        return new CancelarReservaWriteResult(data.CodigoReserva, data.EstadoReserva, data.FechaCancelacionUtc);
    }

    private async Task<T> PostEnvelopeAsync<T>(string relativeUri, object body, CancellationToken ct)
        where T : class
    {
        using var resp = await Http.PostAsJsonAsync(relativeUri, body, JsonOptions, ct);
        return await ReadEnvelopeDataAsync<T>(resp, "POST", relativeUri, ct);
    }

    private async Task<T> PatchEnvelopeAsync<T>(string relativeUri, object body, CancellationToken ct)
        where T : class
    {
        using var content = JsonContent.Create(body, options: JsonOptions);
        using var request = new HttpRequestMessage(HttpMethod.Patch, relativeUri) { Content = content };
        using var resp = await Http.SendAsync(request, ct);
        return await ReadEnvelopeDataAsync<T>(resp, "PATCH", relativeUri, ct);
    }

    private async Task<T> ReadEnvelopeDataAsync<T>(HttpResponseMessage resp, string verb, string relativeUri, CancellationToken ct)
        where T : class
    {
        MsApiEnvelope<T>? envelope = null;
        try
        {
            envelope = await resp.Content.ReadFromJsonAsync<MsApiEnvelope<T>>(JsonOptions, ct);
        }
        catch
        {
            // body no JSON
        }

        if (resp.IsSuccessStatusCode && envelope?.Data is not null && envelope.Success)
        {
            return envelope.Data;
        }

        var status = envelope?.StatusCode > 0 ? envelope.StatusCode : (int)resp.StatusCode;
        var message = envelope?.Message ?? $"Downstream {verb} {Http.BaseAddress}{relativeUri} respondio {status}.";
        Logger.LogWarning("HTTP {Status} desde {Service} {Verb} {Uri}: {Body}",
            status, Http.BaseAddress, verb, relativeUri, await resp.Content.ReadAsStringAsync(ct));

        if (status is 408 or 504)
        {
            throw new TimeoutException(message);
        }

        throw new MicroserviceClientException((HttpStatusCode)status, message);
    }

    private sealed class CrearReservaRestPayload
    {
        public string CodigoReserva { get; set; } = string.Empty;
        public string EstadoReserva { get; set; } = string.Empty;
        public DateTimeOffset FechaReservaUtc { get; set; }
        public int CantidadDias { get; set; }
        public decimal SubtotalVehiculo { get; set; }
        public decimal SubtotalExtras { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
    }

    private sealed class CancelarReservaRestPayload
    {
        public string CodigoReserva { get; set; } = string.Empty;
        public string EstadoReserva { get; set; } = string.Empty;
        public DateTimeOffset FechaCancelacionUtc { get; set; }
    }
}
