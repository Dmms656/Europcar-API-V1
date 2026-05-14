namespace Europcar.Rental.DataManagement.Models;

public class ExtraDetailModel
{
    public int IdExtra { get; set; }
    public string CodigoExtra { get; set; } = string.Empty;
    public string NombreExtra { get; set; } = string.Empty;
    public string TipoExtra { get; set; } = string.Empty;
    public bool RequiereStock { get; set; }
    public decimal ValorFijo { get; set; }
}
