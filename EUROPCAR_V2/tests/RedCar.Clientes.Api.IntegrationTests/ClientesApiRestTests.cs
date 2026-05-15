using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RedCar.Shared.Contracts.Common;
using Xunit;

namespace RedCar.Clientes.Api.IntegrationTests;

public sealed class ClientesApiRestTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public ClientesApiRestTests(WebApplicationFactory<Program> factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task Info_returns_success()
    {
        var r = await _client.GetAsync("/info");
        r.EnsureSuccessStatusCode();
        var body = await r.Content.ReadFromJsonAsync<ApiResponse<ServiceInfo>>(Json);
        Assert.True(body?.Success);
        Assert.Contains("Clientes", body!.Data!.Service, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upsert_cliente_returns_created_or_existing()
    {
        var id = Guid.NewGuid().ToString("N")[..12];
        var payload = new
        {
            nombres = "Integration",
            apellidos = "Test User",
            tipoIdentificacion = "CEDULA",
            numeroIdentificacion = id,
            correo = $"{id}@test.local",
            telefono = "0990000001"
        };

        var r = await _client.PostAsJsonAsync("/api/v1/clientes/upsert", payload, Json);
        r.EnsureSuccessStatusCode();
        var body = await r.Content.ReadFromJsonAsync<ApiResponse<ClienteUpsertData>>(Json);
        Assert.True(body?.Success);
        Assert.NotNull(body?.Data);
        Assert.True(body!.Data!.IdCliente > 0);
    }

    private sealed class ClienteUpsertData
    {
        public int IdCliente { get; set; }
        public Guid ClienteGuid { get; set; }
        public bool Created { get; set; }
    }
}
