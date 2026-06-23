using System.Globalization;

namespace RedCar.Shared.Contracts.Serialization;

/// <summary>
/// Acepta fechas/horas en varios formatos comunes (YYYY-MM-DD, ISO 8601 con zona horaria, HH:mm, etc.).
/// </summary>
public static class FlexibleDateTimeParsing
{
    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd",
        "yyyy/MM/dd",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssK",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
        "O"
    ];

    public static bool TryParseDateOnly(string? value, out DateOnly result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var s = value.Trim();

        if (DateOnly.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
            return true;
        }

        if (DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
            return true;
        }

        if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
        {
            result = DateOnly.FromDateTime(dto.UtcDateTime);
            return true;
        }

        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
        {
            result = DateOnly.FromDateTime(dt);
            return true;
        }

        return false;
    }

    public static DateOnly ParseDateOnly(string value) =>
        TryParseDateOnly(value, out var date)
            ? date
            : throw new FormatException($"No se pudo interpretar la fecha: '{value}'.");

    public static bool TryParseTimeOnly(string? value, out TimeOnly result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var s = value.Trim();

        if (TimeOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
            return true;
        }

        if (TimeOnly.TryParseExact(s, ["HH:mm:ss", "HH:mm", "H:mm:ss", "H:mm"], CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
            return true;
        }

        if (s.Contains('T', StringComparison.Ordinal)
            && DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
        {
            result = TimeOnly.FromDateTime(dto.UtcDateTime);
            return true;
        }

        return false;
    }

    public static TimeOnly ParseTimeOnly(string value) =>
        TryParseTimeOnly(value, out var time)
            ? time
            : throw new FormatException($"No se pudo interpretar la hora: '{value}'.");
}
