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

    public async Task<IReadOnlyList<CiudadDto>?> ListCiudadesAsync(CancellationToken ct = default)
    {
        var envelope = await GetAsync<MsApiEnvelope<IReadOnlyList<CiudadDto>>>("/api/v1/ciudades", ct);
        return envelope?.Data;
    }

    public async Task<IReadOnlyList<PaisDto>?> ListPaisesAsync(CancellationToken ct = default)
    {
        var envelope = await GetAsync<MsApiEnvelope<IReadOnlyList<PaisDto>>>("/api/v1/ciudades/paises", ct);
        return envelope?.Data;
    }

    public Task<LocalizacionDto> CreateLocalizacionAsync(object request, CancellationToken ct = default)
        => PostEnvelopeAsync<LocalizacionDto>("/api/v1/localizaciones", request, ct);

    public Task<LocalizacionDto> UpdateLocalizacionAsync(int id, object request, CancellationToken ct = default)
        => PutAsync<object, LocalizacionDto>($"/api/v1/localizaciones/{id}", request, ct);

    public Task CambiarEstadoLocalizacionAsync(int id, string estado, string? motivo, CancellationToken ct = default)
        => PutVoidAsync($"/api/v1/localizaciones/{id}/estado", new { estado, motivo }, ct);

    public Task DeleteLocalizacionAsync(int id, CancellationToken ct = default)
        => DeleteEnvelopeAsync($"/api/v1/localizaciones/{id}", ct);

    public Task<CiudadDto> CreateCiudadAsync(object request, CancellationToken ct = default)
        => PostEnvelopeAsync<CiudadDto>("/api/v1/ciudades", request, ct);

    public Task<CiudadDto> UpdateCiudadAsync(int id, object request, CancellationToken ct = default)
        => PutAsync<object, CiudadDto>($"/api/v1/ciudades/{id}", request, ct);

    public Task CambiarEstadoCiudadAsync(int id, string estado, CancellationToken ct = default)
        => PutVoidAsync($"/api/v1/ciudades/{id}/estado", new { estado }, ct);

    public Task DeleteCiudadAsync(int id, CancellationToken ct = default)
        => DeleteEnvelopeAsync($"/api/v1/ciudades/{id}", ct);
}
