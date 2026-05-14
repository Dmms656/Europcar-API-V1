using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Middleware.RedCar.Api.Controllers.V1;

/// <summary>
/// BFF: reenvía <c>/api/v1/Auth/*</c> al MS Seguridad para que el SPA solo hable con el middleware (Render).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/Auth")]
public sealed class AuthBffController : ControllerBase
{
    private const string SeguridadClientName = "SeguridadNoBearer";
    private readonly IHttpClientFactory _httpFactory;

    public AuthBffController(IHttpClientFactory httpFactory) => _httpFactory = httpFactory;

    [HttpPost("login")]
    [AllowAnonymous]
    public Task<IActionResult> Login(CancellationToken ct) => ProxyPostAsync("api/v1/Auth/login", ct);

    [HttpPost("register")]
    [AllowAnonymous]
    public Task<IActionResult> Register(CancellationToken ct) => ProxyPostAsync("api/v1/Auth/register", ct);

    [HttpGet("cedula-exists")]
    [AllowAnonymous]
    public Task<IActionResult> CedulaExists(CancellationToken ct)
    {
        var qs = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
        return ProxyGetAsync($"api/v1/Auth/cedula-exists{qs}", ct);
    }

    [HttpPut("profile")]
    [Authorize]
    public IActionResult ProfileNotImplemented()
    {
        return StatusCode(501, new
        {
            success = false,
            statusCode = 501,
            message = "Actualización de perfil aún no está disponible en el nuevo stack.",
            data = (object?)null,
            traceId = HttpContext.TraceIdentifier
        });
    }

    [HttpPut("change-password")]
    [Authorize]
    public IActionResult ChangePasswordNotImplemented()
    {
        return StatusCode(501, new
        {
            success = false,
            statusCode = 501,
            message = "Cambio de contraseña aún no está disponible en el nuevo stack.",
            data = (object?)null,
            traceId = HttpContext.TraceIdentifier
        });
    }

    private async Task<IActionResult> ProxyPostAsync(string relativePath, CancellationToken ct)
    {
        var client = _httpFactory.CreateClient(SeguridadClientName);
        using var content = new StreamContent(Request.Body);
        var hdr = Request.ContentType;
        if (!string.IsNullOrEmpty(hdr))
            content.Headers.TryAddWithoutValidation("Content-Type", hdr);

        using var resp = await client.PostAsync(relativePath, content, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        return new ContentResult
        {
            Content = body,
            StatusCode = (int)resp.StatusCode,
            ContentType = resp.Content.Headers.ContentType?.ToString() ?? "application/json; charset=utf-8"
        };
    }

    private async Task<IActionResult> ProxyGetAsync(string relativePathAndQuery, CancellationToken ct)
    {
        var client = _httpFactory.CreateClient(SeguridadClientName);
        using var resp = await client.GetAsync(relativePathAndQuery, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        return new ContentResult
        {
            Content = body,
            StatusCode = (int)resp.StatusCode,
            ContentType = resp.Content.Headers.ContentType?.ToString() ?? "application/json; charset=utf-8"
        };
    }
}
