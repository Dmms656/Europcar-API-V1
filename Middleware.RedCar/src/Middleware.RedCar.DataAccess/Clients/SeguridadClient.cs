using Microsoft.Extensions.Logging;
using Middleware.RedCar.DataAccess.Clients.Interfaces;

namespace Middleware.RedCar.DataAccess.Clients;

public sealed class SeguridadClient : HttpClientBase, ISeguridadClient
{
    public SeguridadClient(HttpClient http, ILogger<SeguridadClient> logger) : base(http, logger) { }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        try
        {
            using var resp = await Http.GetAsync("/health/live", ct);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public Task<ServiceInfoDto?> GetInfoAsync(CancellationToken ct = default)
    {
        // GET /info esta envuelto por ApiResponse<T> en MS. Aqui leemos el envoltorio
        // y devolvemos solo el .data del payload.
        return GetWrappedAsync<ServiceInfoDto>("/info", ct);
    }

    private async Task<T?> GetWrappedAsync<T>(string uri, CancellationToken ct)
    {
        var envelope = await GetAsync<MsApiEnvelope<T>>(uri, ct);
        return envelope is null ? default : envelope.Data;
    }
}
