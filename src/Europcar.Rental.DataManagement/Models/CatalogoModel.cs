namespace Europcar.Rental.DataManagement.Models;

public class CatalogoModel
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = "ACT";
}
