namespace RedCar.Catalogo.DataAccess.Entities;

public sealed class CategoriaVehiculo
{
    public int IdCategoria { get; set; }
    public string CodigoCategoria { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public string? DescripcionCategoria { get; set; }
    public string EstadoCategoria { get; set; } = "ACT";
    public bool EsEliminado { get; set; }
}
