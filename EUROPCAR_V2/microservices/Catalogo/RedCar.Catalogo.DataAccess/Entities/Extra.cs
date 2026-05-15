namespace RedCar.Catalogo.DataAccess.Entities;

public sealed class Extra
{
    public int IdExtra { get; set; }
    public string CodigoExtra { get; set; } = string.Empty;
    public string NombreExtra { get; set; } = string.Empty;
    public string DescripcionExtra { get; set; } = string.Empty;
    public decimal ValorFijo { get; set; }
    public string EstadoExtra { get; set; } = "ACT";
    public bool EsEliminado { get; set; }
}
