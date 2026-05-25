namespace Europcar.Rental.Api.Extensions;

/// <summary>
/// Cookie HttpOnly para el JWT (mitiga robo por XSS frente a localStorage).
/// </summary>
public static class AuthCookieExtensions
{
    public const string CookieName = "rc_auth";

    public static void SetAuthCookie(HttpResponse response, string token, DateTime expirationUtc, bool secure)
    {
        response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            Expires = new DateTimeOffset(expirationUtc, TimeSpan.Zero),
            Path = "/"
        });
    }

    public static void ClearAuthCookie(HttpResponse response)
    {
        response.Cookies.Delete(CookieName, new CookieOptions { Path = "/" });
    }
}
