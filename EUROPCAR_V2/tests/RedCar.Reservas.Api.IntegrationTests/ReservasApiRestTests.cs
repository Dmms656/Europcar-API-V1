using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RedCar.Shared.Contracts.Common;
using Xunit;

namespace RedCar.Reservas.Api.IntegrationTests;

public sealed class ReservasApiRestTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public ReservasApiRestTests(WebApplicationFactory<Program> factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task Info_returns_success()
    {
        var r = await _client.GetAsync("/info");
        r.EnsureSuccessStatusCode();
        var body = await r.Content.ReadFromJsonAsync<ApiResponse<ServiceInfo>>(Json);
        Assert.True(body?.Success);
        Assert.Contains("Reservas", body!.Data!.Service, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Disponibilidad_returns_envelope()
    {
        var fr = Uri.EscapeDataString(new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero).ToString("O"));
        var fd = Uri.EscapeDataString(new DateTimeOffset(2026, 6, 5, 10, 0, 0, TimeSpan.Zero).ToString("O"));
        var uri = $"/api/v1/reservas/disponibilidad?idVehiculo=1&idLocalizacion=1&fechaRecogida={fr}&fechaDevolucion={fd}";
        var r = await _client.GetAsync(uri);
        r.EnsureSuccessStatusCode();
        var body = await r.Content.ReadFromJsonAsync<ApiResponse<DisponibilidadData>>(Json);
        Assert.True(body?.Success);
        Assert.NotNull(body?.Data);
        Assert.Equal(1, body!.Data!.IdVehiculo);
    }

    [Fact]
    public async Task Reserva_unknown_returns_404()
    {
        var r = await _client.GetAsync("/api/v1/reservas/NO-EXISTE-999");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, r.StatusCode);
    }

    private sealed class DisponibilidadData
    {
        public int IdVehiculo { get; set; }
        public int IdLocalizacion { get; set; }
        public bool Disponible { get; set; }
    }
}
