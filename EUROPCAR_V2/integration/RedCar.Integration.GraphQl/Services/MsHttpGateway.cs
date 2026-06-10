using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RedCar.Integration.GraphQl.Services;

/// <summary>Cliente HTTP interno hacia los microservicios REST existentes (sin modificar sus endpoints).</summary>
public sealed class MsHttpGateway
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpFactory;
    private readonly MicroserviciosGatewaySettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MsHttpGateway(
        IHttpClientFactory httpFactory,
        Microsoft.Extensions.Options.IOptions<MicroserviciosGatewaySettings> settings,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpFactory = httpFactory;
        _settings = settings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<T?> GetCatalogoAsync<T>(string path, CancellationToken ct) =>
        GetAsync<T>(_settings.Catalogo.BaseUrl, path, ct);

    public Task<T?> GetLocalizacionesAsync<T>(string path, CancellationToken ct) =>
        GetAsync<T>(_settings.Localizaciones.BaseUrl, path, ct);

    public Task<T?> GetReservasAsync<T>(string path, CancellationToken ct) =>
        GetAsync<T>(_settings.Reservas.BaseUrl, path, ct);

    private async Task<T?> GetAsync<T>(string baseUrl, string path, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return default;

        var client = _httpFactory.CreateClient(nameof(MsHttpGateway));
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(60);

        using var req = new HttpRequestMessage(HttpMethod.Get, path.TrimStart('/'));
        PropagateBearer(req);

        using var res = await client.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode) return default;

        var envelope = await res.Content.ReadFromJsonAsync<ApiEnvelope<T>>(Json, ct);
        return envelope is null ? default : envelope.Data;
    }

    private void PropagateBearer(HttpRequestMessage req)
    {
        var auth = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(auth))
            req.Headers.Authorization = AuthenticationHeaderValue.Parse(auth);
    }

    private sealed class ApiEnvelope<T>
    {
        public T? Data { get; set; }
    }
}
