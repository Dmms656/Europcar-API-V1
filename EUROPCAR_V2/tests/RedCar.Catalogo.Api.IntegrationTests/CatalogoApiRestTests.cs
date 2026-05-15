using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RedCar.Shared.Contracts.Common;
using Xunit;

namespace RedCar.Catalogo.Api.IntegrationTests;

public sealed class CatalogoApiRestTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public CatalogoApiRestTests(WebApplicationFactory<Program> factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task Info_returns_success_and_catalogo_schema()
    {
        var r = await _client.GetAsync("/info");
        r.EnsureSuccessStatusCode();
        var body = await r.Content.ReadFromJsonAsync<ApiResponse<ServiceInfo>>(Json);
        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.Equal(200, body.StatusCode);
        Assert.NotNull(body.Data);
        Assert.Contains("Catalogo", body.Data!.Service, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("catalogo", body.Data.Schema, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Vehiculos_list_returns_envelope_with_items_array()
    {
        var fr = Uri.EscapeDataString(new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero).ToString("O"));
        var fd = Uri.EscapeDataString(new DateTimeOffset(2026, 6, 5, 10, 0, 0, TimeSpan.Zero).ToString("O"));
        var uri = $"/api/v1/vehiculos?idLocalizacion=1&fechaRecogida={fr}&fechaDevolucion={fd}&page=1&limit=10";
        var r = await _client.GetAsync(uri);
        r.EnsureSuccessStatusCode();
        using var doc = await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync());
        var root = doc.RootElement;
        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.True(root.TryGetProperty("data", out var data));
        Assert.True(data.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array);
    }

    [Fact]
    public async Task Vehiculo_unknown_returns_404()
    {
        var r = await _client.GetAsync("/api/v1/vehiculos/999999");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task Categorias_list_returns_success()
    {
        var r = await _client.GetAsync("/api/v1/categorias?page=1&limit=10");
        r.EnsureSuccessStatusCode();
        var body = await r.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(Json);
        Assert.NotNull(body?.Data);
        Assert.True(body!.Success);
        Assert.Equal(JsonValueKind.Array, body.Data!.GetProperty("items").ValueKind);
    }

    [Fact]
    public async Task Extras_by_ids_without_query_returns_success()
    {
        var r = await _client.GetAsync("/api/v1/extras/by-ids");
        r.EnsureSuccessStatusCode();
        using var doc = await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync());
        Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
    }
}
