namespace RedCar.Catalogo.DataAccess.Entities;

public sealed class MarcaVehiculo
{
    public int IdMarca { get; set; }
    public string CodigoMarca { get; set; } = string.Empty;
    public string NombreMarca { get; set; } = string.Empty;
    public string EstadoMarca { get; set; } = "ACT";
    public bool EsEliminado { get; set; }
}
