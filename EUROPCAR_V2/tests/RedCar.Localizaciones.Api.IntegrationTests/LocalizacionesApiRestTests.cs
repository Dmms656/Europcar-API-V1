using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RedCar.Shared.Contracts.Common;
using Xunit;

namespace RedCar.Localizaciones.Api.IntegrationTests;

public sealed class LocalizacionesApiRestTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public LocalizacionesApiRestTests(WebApplicationFactory<Program> factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task Info_returns_success()
    {
        var r = await _client.GetAsync("/info");
        r.EnsureSuccessStatusCode();
        var body = await r.Content.ReadFromJsonAsync<ApiResponse<ServiceInfo>>(Json);
        Assert.True(body?.Success);
        Assert.Contains("Localizaciones", body!.Data!.Service, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task List_returns_items_array()
    {
        var r = await _client.GetAsync("/api/v1/localizaciones?page=1&limit=10");
        r.EnsureSuccessStatusCode();
        using var doc = await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync());
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(JsonValueKind.Array, data.GetProperty("items").ValueKind);
    }

    [Fact]
    public async Task Get_unknown_returns_404()
    {
        var r = await _client.GetAsync("/api/v1/localizaciones/999999");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, r.StatusCode);
    }
}
