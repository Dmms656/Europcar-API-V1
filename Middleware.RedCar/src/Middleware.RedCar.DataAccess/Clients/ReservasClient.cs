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
}
