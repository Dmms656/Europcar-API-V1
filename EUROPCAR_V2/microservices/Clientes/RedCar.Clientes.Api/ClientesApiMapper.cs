namespace RedCar.Clientes.Api;

internal static class ClientesApiMapper
{
    public static string ToDbTipoIdentificacion(string t)
    {
        var x = (t ?? string.Empty).Trim().ToUpperInvariant();
        return x switch
        {
            "CEDULA" => "CED",
            "PASAPORTE" => "PAS",
            "RUC" => "RUC",
            "CED" => "CED",
            "PAS" => "PAS",
            _ => x.Length <= 10 ? x : "CED"
        };
    }

    public static string ToApiTipoIdentificacion(string db)
    {
        var x = (db ?? string.Empty).Trim().ToUpperInvariant();
        return x switch
        {
            "CED" => "CEDULA",
            "PAS" => "PASAPORTE",
            "RUC" => "RUC",
            _ => x
        };
    }

    public static (string First, string? Second) SplitTwo(string s)
    {
        s = (s ?? string.Empty).Trim();
        if (s.Length == 0) return (string.Empty, null);
        var i = s.IndexOf(' ');
        if (i < 0) return (s, null);
        var second = s[(i + 1)..].Trim();
        return (s[..i], string.IsNullOrEmpty(second) ? null : second);
    }

    public static string JoinNames(string a, string? b)
    {
        a = (a ?? string.Empty).Trim();
        b = string.IsNullOrWhiteSpace(b) ? null : b.Trim();
        return b is null ? a : $"{a} {b}";
    }
}
