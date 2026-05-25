namespace Middleware.RedCar.Api.Extensions;

/// <summary>Cookie HttpOnly para JWT (SPA sin localStorage del token).</summary>
public static class AuthCookieExtensions
{
    public const string CookieName = "rc_auth";

    public static void SetAuthCookie(HttpRequest request, HttpResponse response, string token, DateTime expirationUtc)
    {
        var secure = UseSecureCookies(request);
        response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = secure ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = new DateTimeOffset(expirationUtc, TimeSpan.Zero),
            Path = "/"
        });
    }

    public static void ClearAuthCookie(HttpRequest request, HttpResponse response)
    {
        var secure = UseSecureCookies(request);
        response.Cookies.Delete(CookieName, new CookieOptions
        {
            Path = "/",
            Secure = secure,
            SameSite = secure ? SameSiteMode.None : SameSiteMode.Lax
        });
    }

    /// <summary>Render/proxies terminan TLS; sin esto la cookie queda Lax sin Secure y el navegador no la envía.</summary>
    private static bool UseSecureCookies(HttpRequest request)
    {
        if (request.IsHttps) return true;
        var proto = request.Headers["X-Forwarded-Proto"].ToString();
        return string.Equals(proto, "https", StringComparison.OrdinalIgnoreCase);
    }
}
