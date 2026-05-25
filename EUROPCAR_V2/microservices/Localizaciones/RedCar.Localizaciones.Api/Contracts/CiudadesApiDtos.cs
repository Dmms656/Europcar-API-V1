namespace RedCar.Localizaciones.Api.Contracts;

public sealed class CiudadDto
{
    public int IdCiudad { get; init; }
    public Guid CiudadGuid { get; init; }
    public int IdPais { get; init; }
    public string NombreCiudad { get; init; } = string.Empty;
    public string EstadoCiudad { get; init; } = string.Empty;
}

public sealed class PaisDto
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Estado { get; init; } = "ACT";
}
