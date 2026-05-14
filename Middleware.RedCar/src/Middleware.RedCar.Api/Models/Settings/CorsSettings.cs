namespace Middleware.RedCar.Api.Models.Settings;

/// <summary>
/// Orígenes permitidos para CORS (mismo criterio que el monolito FrontendPolicy).
/// Si el array está vacío, se usa AllowAnyOrigin (solo desarrollo).
/// </summary>
public sealed class CorsSettings
{
    public const string SectionName = "Cors";

    /// <summary>URLs exactas del frontend (ej. http://localhost:5173).</summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
