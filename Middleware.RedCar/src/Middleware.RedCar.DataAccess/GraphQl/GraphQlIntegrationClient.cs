using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedCar.Shared.Messaging;

namespace Middleware.RedCar.DataAccess.GraphQl;

/// <summary>Cliente único GraphQL hacia RedCar.Integration.GraphQl (sustituye HTTP disperso en lecturas).</summary>
public sealed class GraphQlIntegrationClient
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly IntegrationSettings _settings;
    private readonly ILogger<GraphQlIntegrationClient> _logger;

    public GraphQlIntegrationClient(
        HttpClient http,
        IOptions<IntegrationSettings> settings,
        ILogger<GraphQlIntegrationClient> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<T?> QueryAsync<T>(string query, object? variables = null, CancellationToken ct = default)
    {
        if (!_settings.UseGraphQl)
            return default;

        var endpoint = ResolveGraphQlEndpoint();
        if (string.IsNullOrWhiteSpace(endpoint))
            return default;

        var body = JsonSerializer.Serialize(new { query, variables }, Json);
        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        using var res = await _http.SendAsync(req, ct);
        var json = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
        {
            _logger.LogWarning("GraphQL error {Status}: {Body}", res.StatusCode, json);
            return default;
        }

        var envelope = JsonSerializer.Deserialize<GraphQlResponse<T>>(json, Json);
        if (envelope?.Errors?.Count > 0)
        {
            _logger.LogWarning("GraphQL errors: {Errors}", string.Join("; ", envelope.Errors.Select(e => e.Message)));
            return default;
        }

        return envelope is null ? default : envelope.Data;
    }

    private string? ResolveGraphQlEndpoint()
    {
        if (_settings.EmbeddedGraphQl)
            return string.IsNullOrWhiteSpace(_settings.GraphQlBaseUrl)
                ? "http://127.0.0.1:8080/graphql"
                : _settings.GraphQlBaseUrl;

        return string.IsNullOrWhiteSpace(_settings.GraphQlBaseUrl) ? null : _settings.GraphQlBaseUrl;
    }

    private sealed class GraphQlResponse<T>
    {
        public T? Data { get; set; }
        public List<GraphQlError>? Errors { get; set; }
    }

    private sealed class GraphQlError
    {
        public string Message { get; set; } = string.Empty;
    }
}

public static class GraphQlQueries
{
    public const string Vehiculo = """
        query($id: Int!) {
          vehiculo(id: $id) {
            idVehiculo codigoInterno marca modelo idLocalizacion estado precioDia nombreCategoria transmision
          }
        }
        """;

    public const string Disponibilidad = """
        query($input: DisponibilidadInput!) {
          disponibilidad(input: $input) { disponible idVehiculo idLocalizacion }
        }
        """;

    public const string Localizacion = """
        query($id: Int!) {
          localizacion(id: $id) {
            idLocalizacion nombre codigo direccion idCiudad ciudadNombre
          }
        }
        """;
}
