namespace Middleware.RedCar.Api.Models.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "redcar-v2";
    public string Audience { get; set; } = "redcar-v2-clients";
    public int ExpirationMinutes { get; set; } = 60;
}
