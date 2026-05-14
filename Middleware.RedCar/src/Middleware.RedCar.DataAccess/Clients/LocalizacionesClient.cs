using Microsoft.Extensions.Logging;
using Middleware.RedCar.DataAccess.Clients.Interfaces;

namespace Middleware.RedCar.DataAccess.Clients;

public sealed class LocalizacionesClient : HttpClientBase, ILocalizacionesClient
{
    public LocalizacionesClient(HttpClient http, ILogger<LocalizacionesClient> logger) : base(http, logger) { }

    public async Task<PagedDto<LocalizacionDto>?> ListLocalizacionesAsync(int? idCiudad, int page, int limit, CancellationToken ct = default)
    {
        var qs = new List<string> { $"page={page}", $"limit={limit}" };
        if (idCiudad.HasValue) qs.Add($"idCiudad={idCiudad.Value}");

        var envelope = await GetAsync<MsApiEnvelope<PagedDto<LocalizacionDto>>>(
            "/api/v1/localizaciones?" + string.Join('&', qs), ct);
        return envelope?.Data;
    }

    public async Task<LocalizacionDto?> GetLocalizacionAsync(int idLocalizacion, CancellationToken ct = default)
    {
        var envelope = await GetAsync<MsApiEnvelope<LocalizacionDto>>(
            $"/api/v1/localizaciones/{idLocalizacion}", ct);
        return envelope?.Data;
    }
}
