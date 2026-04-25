namespace Europcar.Rental.Api.Models.Settings;

/// <summary>
/// Configuración JWT mapeada desde appsettings.json.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Europcar.Rental.Api";
    public string Audience { get; set; } = "Europcar.Rental.Client";
    public int ExpirationMinutes { get; set; } = 60;
}

/// <summary>
/// Configuración de CORS mapeada desde appsettings.json.
/// </summary>
public class CorsSettings
{
    public const string SectionName = "CorsSettings";

    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
    public string[] AllowedHeaders { get; set; } = new[] { "Authorization", "Content-Type" };
}
