using Microsoft.Extensions.Logging;
using Middleware.RedCar.DataAccess.Clients.Interfaces;

namespace Middleware.RedCar.DataAccess.Clients;

public sealed class ClientesClient : HttpClientBase, IClientesClient
{
    public ClientesClient(HttpClient http, ILogger<ClientesClient> logger) : base(http, logger) { }

    public async Task<ClienteUpsertResult?> UpsertClienteAsync(ClienteUpsertRequest req, CancellationToken ct = default)
    {
        var envelope = await PostAsync<ClienteUpsertRequest, MsApiEnvelope<ClienteUpsertResult>>(
            "/api/v1/clientes/upsert", req, ct);
        return envelope?.Data;
    }

    public async Task<IReadOnlyList<ClienteListItemDto>?> ListAllAsync(int page = 1, int limit = 500, CancellationToken ct = default)
    {
        var envelope = await GetAsync<MsApiEnvelope<IReadOnlyList<ClienteListItemDto>>>(
            $"/api/v1/clientes?page={page}&limit={limit}", ct);
        return envelope?.Data;
    }

    public async Task<ClienteDetalleDto?> GetByIdAsync(int idCliente, CancellationToken ct = default)
    {
        var envelope = await GetAsync<MsApiEnvelope<ClienteDetalleDto>>($"/api/v1/clientes/{idCliente}", ct);
        return envelope?.Data;
    }

    public async Task<ClienteDetalleDto?> GetByIdentificacionAsync(string numeroIdentificacion, CancellationToken ct = default)
    {
        var doc = Uri.EscapeDataString((numeroIdentificacion ?? string.Empty).Trim());
        if (string.IsNullOrEmpty(doc)) return null;
        var envelope = await GetAsync<MsApiEnvelope<ClienteDetalleDto>>($"/api/v1/clientes/by-identificacion/{doc}", ct);
        return envelope?.Data;
    }

    public Task<ClienteListItemDto> CreateClienteAsync(object request, CancellationToken ct = default)
        => PostEnvelopeAsync<ClienteListItemDto>("/api/v1/clientes", request, ct);

    public Task<ClienteListItemDto> UpdateClienteAsync(int id, object request, CancellationToken ct = default)
        => PutAsync<object, ClienteListItemDto>($"/api/v1/clientes/{id}", request, ct);

    public Task DeleteClienteAsync(int id, CancellationToken ct = default)
        => DeleteEnvelopeAsync($"/api/v1/clientes/{id}", ct);

    public async Task<IReadOnlyList<ConductorUpsertResult>?> UpsertConductoresAsync(int idCliente, IReadOnlyList<ConductorUpsertRequest> conductores, CancellationToken ct = default)
    {
        var envelope = await PostAsync<IReadOnlyList<ConductorUpsertRequest>, MsApiEnvelope<IReadOnlyList<ConductorUpsertResult>>>(
            $"/api/v1/clientes/{idCliente}/conductores/upsert", conductores, ct);
        return envelope?.Data;
    }
}
