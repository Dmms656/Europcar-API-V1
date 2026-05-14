namespace Middleware.RedCar.DataManagement.Models.Catalogo;

/// <summary>
/// Vista interna del vehiculo dentro del middleware, ya con el
/// precio base por dia listo para calcular subtotales.
/// </summary>
public sealed record VehiculoDataModel
{
    public required int IdVehiculo { get; init; }
    public required string CodigoInterno { get; init; }
    public required int IdMarca { get; init; }
    public required string Marca { get; init; }
    public required int IdCategoria { get; init; }
    public required string CategoriaCodigo { get; init; }
    public required string CategoriaNombre { get; init; }
    public required string Modelo { get; init; }
    public required int Anio { get; init; }
    public required string Color { get; init; }
    public required string ImagenUrl { get; init; }
    public required string Transmision { get; init; }
    public required string Combustible { get; init; }
    public required int CapacidadPasajeros { get; init; }
    public required int CapacidadMaletas { get; init; }
    public required int NumeroPuertas { get; init; }
    public required bool AireAcondicionado { get; init; }
    public required string Estado { get; init; }
    public required int IdLocalizacion { get; init; }
    public required decimal PrecioBaseDia { get; init; }
}
