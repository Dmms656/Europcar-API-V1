namespace RedCar.Integration.GraphQl.Graph.Types;

public sealed class VehiculoGql
{
    public int IdVehiculo { get; set; }
    public string CodigoInterno { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int IdLocalizacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal PrecioDia { get; set; }
    public string? NombreCategoria { get; set; }
    public string? Transmision { get; set; }
    public bool? Disponible { get; set; }
}

public sealed class VehiculoPagedGql
{
    public IReadOnlyList<VehiculoGql> Items { get; set; } = Array.Empty<VehiculoGql>();
    public int TotalElementos { get; set; }
    public int PaginaActual { get; set; }
    public int ElementosPorPagina { get; set; }
}

public sealed class LocalizacionGql
{
    public int IdLocalizacion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public int IdCiudad { get; set; }
    public string CiudadNombre { get; set; } = string.Empty;
}

public sealed class LocalizacionPagedGql
{
    public IReadOnlyList<LocalizacionGql> Items { get; set; } = Array.Empty<LocalizacionGql>();
    public int TotalElementos { get; set; }
}

public sealed class DisponibilidadGql
{
    public int IdVehiculo { get; set; }
    public int IdLocalizacion { get; set; }
    public bool Disponible { get; set; }
}

public sealed class CategoriaGql
{
    public int IdCategoria { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public sealed class CategoriaPagedGql
{
    public IReadOnlyList<CategoriaGql> Items { get; set; } = Array.Empty<CategoriaGql>();
    public int TotalElementos { get; set; }
}

public sealed class ExtraGql
{
    public int IdExtra { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public sealed class ExtraPagedGql
{
    public IReadOnlyList<ExtraGql> Items { get; set; } = Array.Empty<ExtraGql>();
    public int TotalElementos { get; set; }
}

public sealed class ReservaGql
{
    public string CodigoReserva { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public int IdVehiculo { get; set; }
}

public sealed class FacturaGql
{
    public string NumeroFactura { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string CodigoReserva { get; set; } = string.Empty;
}

public sealed class VehiculoFiltroInput
{
    public int IdLocalizacion { get; set; }
    public DateTimeOffset FechaRecogida { get; set; }
    public DateTimeOffset FechaDevolucion { get; set; }
    public string? NombreCategoria { get; set; }
    public string? NombreMarca { get; set; }
    public string? Transmision { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

public sealed class DisponibilidadInput
{
    public int IdVehiculo { get; set; }
    public int IdLocalizacion { get; set; }
    public DateTimeOffset FechaRecogida { get; set; }
    public DateTimeOffset FechaDevolucion { get; set; }
}
