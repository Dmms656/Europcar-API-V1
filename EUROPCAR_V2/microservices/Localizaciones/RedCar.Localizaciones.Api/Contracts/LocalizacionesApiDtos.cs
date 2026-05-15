namespace RedCar.Localizaciones.Api.Contracts;

public sealed class PagedDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int PaginaActual { get; init; }
    public int TotalPaginas { get; init; }
    public int TotalElementos { get; init; }
    public int ElementosPorPagina { get; init; }
}

public sealed class LocalizacionDto
{
    public int IdLocalizacion { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public string HorarioAtencion { get; init; } = string.Empty;
    public string ZonaHoraria { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public int IdCiudad { get; init; }
    public string CiudadNombre { get; init; } = string.Empty;
}
