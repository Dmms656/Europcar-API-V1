using System.Globalization;
using Microsoft.Extensions.Logging;
using Middleware.RedCar.DataAccess.Clients.Interfaces;

namespace Middleware.RedCar.DataAccess.Clients;

public sealed class CatalogoClient : HttpClientBase, ICatalogoClient
{
    public CatalogoClient(HttpClient http, ILogger<CatalogoClient> logger) : base(http, logger) { }

    public async Task<PagedDto<VehiculoCatalogoDto>?> BuscarVehiculosAsync(VehiculoQuery query, CancellationToken ct = default)
    {
        var qs = new List<string>
        {
            $"idLocalizacion={query.IdLocalizacion}",
            $"fechaRecogida={Uri.EscapeDataString(query.FechaRecogida.ToString("O"))}",
            $"fechaDevolucion={Uri.EscapeDataString(query.FechaDevolucion.ToString("O"))}",
            $"page={query.Page}",
            $"limit={query.Limit}"
        };
        if (!string.IsNullOrWhiteSpace(query.NombreCategoria)) qs.Add($"nombreCategoria={Uri.EscapeDataString(query.NombreCategoria!)}");
        if (!string.IsNullOrWhiteSpace(query.NombreMarca))     qs.Add($"nombreMarca={Uri.EscapeDataString(query.NombreMarca!)}");
        if (!string.IsNullOrWhiteSpace(query.Transmision))     qs.Add($"transmision={Uri.EscapeDataString(query.Transmision!)}");
        if (!string.IsNullOrWhiteSpace(query.Sort))            qs.Add($"sort={Uri.EscapeDataString(query.Sort!)}");

        var uri = "/api/v1/vehiculos?" + string.Join('&', qs);
        var envelope = await GetAsync<MsApiEnvelope<PagedDto<VehiculoCatalogoDto>>>(uri, ct);
        return envelope?.Data;
    }

    public async Task<VehiculoCatalogoDto?> GetVehiculoAsync(int idVehiculo, CancellationToken ct = default)
    {
        var envelope = await GetAsync<MsApiEnvelope<VehiculoCatalogoDto>>($"/api/v1/vehiculos/{idVehiculo}", ct);
        return envelope?.Data;
    }

    public async Task<PagedDto<CategoriaDto>?> ListCategoriasAsync(int page, int limit, CancellationToken ct = default)
    {
        var envelope = await GetAsync<MsApiEnvelope<PagedDto<CategoriaDto>>>(
            $"/api/v1/categorias?page={page}&limit={limit}", ct);
        return envelope?.Data;
    }

    public async Task<PagedDto<ExtraDto>?> ListExtrasAsync(int page, int limit, CancellationToken ct = default)
    {
        var envelope = await GetAsync<MsApiEnvelope<PagedDto<ExtraDto>>>(
            $"/api/v1/extras?page={page}&limit={limit}", ct);
        return envelope?.Data;
    }

    public async Task<IReadOnlyList<ExtraDto>?> GetExtrasByIdsAsync(IEnumerable<int> idExtras, CancellationToken ct = default)
    {
        var ids = string.Join(',', idExtras.Select(i => i.ToString(CultureInfo.InvariantCulture)));
        if (string.IsNullOrEmpty(ids)) return Array.Empty<ExtraDto>();

        var envelope = await GetAsync<MsApiEnvelope<IReadOnlyList<ExtraDto>>>(
            $"/api/v1/extras/by-ids?ids={ids}", ct);
        return envelope?.Data;
    }
}
