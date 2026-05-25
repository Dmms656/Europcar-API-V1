using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Middleware.RedCar.DataAccess.Clients;

/// <summary>
/// Base para todos los HttpClients tipados del middleware.
/// Encapsula serializacion JSON con camelCase, manejo de 404 y logging consistente.
/// El JWT del usuario llamante se inyecta via DelegatingHandler en Program.cs
/// (no lo manejamos aqui para no acoplar cada cliente a HttpContext).
/// </summary>
public abstract class HttpClientBase
{
    protected readonly HttpClient Http;
    protected readonly ILogger Logger;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    protected HttpClientBase(HttpClient http, ILogger logger)
    {
        Http = http;
        Logger = logger;
    }

    /// <summary>
    /// GET tipado. Devuelve null en 404, lanza en otros errores.
    /// </summary>
    protected async Task<T?> GetAsync<T>(string relativeUri, CancellationToken ct)
    {
        try
        {
            using var resp = await Http.GetAsync(relativeUri, ct);
            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                Logger.LogWarning("HTTP {Status} desde {Service} {Uri}: {Body}",
                    (int)resp.StatusCode, Http.BaseAddress, relativeUri, body);
                if (resp.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout)
                {
                    throw new TimeoutException($"Timeout downstream {Http.BaseAddress}{relativeUri} respondio {(int)resp.StatusCode}.");
                }
                throw new HttpRequestException($"Downstream {Http.BaseAddress}{relativeUri} respondio {(int)resp.StatusCode}.");
            }

            return await resp.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
        }
        catch (TaskCanceledException tce) when (!ct.IsCancellationRequested)
        {
            Logger.LogWarning(tce, "Timeout llamando {Uri}", relativeUri);
            throw new TimeoutException($"Timeout llamando {Http.BaseAddress}{relativeUri}", tce);
        }
    }

    /// <summary>
    /// POST tipado. Devuelve la respuesta deserializada o null si la API responde 204.
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string relativeUri, TRequest body, CancellationToken ct)
    {
        try
        {
            using var resp = await Http.PostAsJsonAsync(relativeUri, body, JsonOptions, ct);
            if (resp.StatusCode == HttpStatusCode.NoContent)
            {
                return default;
            }

            if (!resp.IsSuccessStatusCode)
            {
                var responseBody = await resp.Content.ReadAsStringAsync(ct);
                Logger.LogWarning("HTTP {Status} desde {Service} POST {Uri}: {Body}",
                    (int)resp.StatusCode, Http.BaseAddress, relativeUri, responseBody);
                if (resp.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout)
                {
                    throw new TimeoutException($"Timeout downstream POST {Http.BaseAddress}{relativeUri} respondio {(int)resp.StatusCode}.");
                }
                throw new HttpRequestException($"Downstream POST {Http.BaseAddress}{relativeUri} respondio {(int)resp.StatusCode}.");
            }

            return await resp.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);
        }
        catch (TaskCanceledException tce) when (!ct.IsCancellationRequested)
        {
            Logger.LogWarning(tce, "Timeout en POST {Uri}", relativeUri);
            throw new TimeoutException($"Timeout llamando POST {Http.BaseAddress}{relativeUri}", tce);
        }
    }

    /// <summary>
    /// PATCH tipado. Mismo comportamiento que POST.
    /// </summary>
    protected async Task<TResponse?> PatchAsync<TRequest, TResponse>(string relativeUri, TRequest body, CancellationToken ct)
    {
        using var content = JsonContent.Create(body, options: JsonOptions);
        using var request = new HttpRequestMessage(HttpMethod.Patch, relativeUri) { Content = content };
        using var resp = await Http.SendAsync(request, ct);

        if (resp.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        if (!resp.IsSuccessStatusCode)
        {
            var responseBody = await resp.Content.ReadAsStringAsync(ct);
            Logger.LogWarning("HTTP {Status} desde {Service} PATCH {Uri}: {Body}",
                (int)resp.StatusCode, Http.BaseAddress, relativeUri, responseBody);
            if (resp.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout)
            {
                throw new TimeoutException($"Timeout downstream PATCH {Http.BaseAddress}{relativeUri} respondio {(int)resp.StatusCode}.");
            }
            throw new HttpRequestException($"Downstream PATCH {Http.BaseAddress}{relativeUri} respondio {(int)resp.StatusCode}.");
        }

        return await resp.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);
    }

    protected async Task<T> PutAsync<TRequest, T>(string relativeUri, TRequest body, CancellationToken ct)
        where T : class
        => await SendEnvelopeAsync<T>(HttpMethod.Put, relativeUri, body, ct);

    protected async Task<T> PostEnvelopeAsync<T>(string relativeUri, object body, CancellationToken ct)
        where T : class
        => await SendEnvelopeAsync<T>(HttpMethod.Post, relativeUri, body, ct);

    protected async Task DeleteEnvelopeAsync(string relativeUri, CancellationToken ct)
    {
        using var resp = await Http.DeleteAsync(relativeUri, ct);
        await EnsureSuccessEnvelopeAsync(resp, "DELETE", relativeUri, ct);
    }

    protected Task PutVoidAsync<TRequest>(string relativeUri, TRequest body, CancellationToken ct)
        => SendEnvelopeVoidAsync(HttpMethod.Put, relativeUri, body, ct);

    private async Task SendEnvelopeVoidAsync(HttpMethod method, string relativeUri, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, relativeUri);
        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);
        using var resp = await Http.SendAsync(request, ct);
        await EnsureSuccessEnvelopeAsync(resp, method.Method, relativeUri, ct);
    }

    private async Task<T> SendEnvelopeAsync<T>(HttpMethod method, string relativeUri, object? body, CancellationToken ct)
        where T : class
    {
        using var request = new HttpRequestMessage(method, relativeUri);
        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        using var resp = await Http.SendAsync(request, ct);
        return await ReadEnvelopeDataAsync<T>(resp, method.Method, relativeUri, ct);
    }

    private async Task EnsureSuccessEnvelopeAsync(HttpResponseMessage resp, string verb, string relativeUri, CancellationToken ct)
    {
        MsApiEnvelope<object>? envelope = null;
        try { envelope = await resp.Content.ReadFromJsonAsync<MsApiEnvelope<object>>(JsonOptions, ct); }
        catch { /* ignore */ }

        if (resp.IsSuccessStatusCode && (envelope is null || envelope.Success))
            return;

        var status = envelope?.StatusCode > 0 ? envelope.StatusCode : (int)resp.StatusCode;
        var message = envelope?.Message ?? $"Downstream {verb} {Http.BaseAddress}{relativeUri} respondio {status}.";
        throw new MicroserviceClientException((HttpStatusCode)status, message);
    }

    private async Task<T> ReadEnvelopeDataAsync<T>(HttpResponseMessage resp, string verb, string relativeUri, CancellationToken ct)
        where T : class
    {
        MsApiEnvelope<T>? envelope = null;
        try { envelope = await resp.Content.ReadFromJsonAsync<MsApiEnvelope<T>>(JsonOptions, ct); }
        catch { /* ignore */ }

        if (resp.IsSuccessStatusCode && envelope?.Data is not null && envelope.Success)
            return envelope.Data;

        var status = envelope?.StatusCode > 0 ? envelope.StatusCode : (int)resp.StatusCode;
        var message = envelope?.Message ?? $"Downstream {verb} {Http.BaseAddress}{relativeUri} respondio {status}.";
        Logger.LogWarning("HTTP {Status} desde {Service} {Verb} {Uri}: {Body}",
            status, Http.BaseAddress, verb, relativeUri, await resp.Content.ReadAsStringAsync(ct));

        if (status is 408 or 504)
            throw new TimeoutException(message);

        throw new MicroserviceClientException((HttpStatusCode)status, message);
    }
}

/// <summary>
/// JSON del envoltorio de los microservicios RedCar (success, statusCode, message, data).
/// </summary>
internal sealed record MsApiEnvelope<T>(bool Success, int StatusCode, string? Message, T? Data, string? TraceId);
