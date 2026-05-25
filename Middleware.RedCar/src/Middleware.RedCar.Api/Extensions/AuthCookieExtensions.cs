namespace Middleware.RedCar.Api.Extensions;

/// <summary>Cookie HttpOnly para JWT (SPA sin localStorage del token).</summary>
public static class AuthCookieExtensions
{
    public const string CookieName = "rc_auth";

    public static void SetAuthCookie(HttpResponse response, string token, DateTime expirationUtc, bool secure)
    {
        response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = secure ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = new DateTimeOffset(expirationUtc, TimeSpan.Zero),
            Path = "/"
        });
    }

    public static void ClearAuthCookie(HttpResponse response, bool secure)
    {
        response.Cookies.Delete(CookieName, new CookieOptions
        {
            Path = "/",
            Secure = secure,
            SameSite = secure ? SameSiteMode.None : SameSiteMode.Lax
        });
    }
}
