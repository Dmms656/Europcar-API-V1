namespace Europcar.Rental.DataManagement.Models;

public class ReservaExtraModel
{
    public int IdReservaExtra { get; set; }
    public int IdExtra { get; set; }
    public string CodigoExtra { get; set; } = string.Empty;
    public string NombreExtra { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
