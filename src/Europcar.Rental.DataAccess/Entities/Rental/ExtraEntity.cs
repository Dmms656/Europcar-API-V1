using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class ExtraEntity : BaseEstadoEntity
{
    public int IdExtra { get; set; }
    public Guid ExtraGuid { get; set; }
    public string CodigoExtra { get; set; } = string.Empty;
    public string NombreExtra { get; set; } = string.Empty;
    public string DescripcionExtra { get; set; } = string.Empty;
    public string TipoExtra { get; set; } = "SERVICIO";
    public bool RequiereStock { get; set; } = false;
    public decimal ValorFijo { get; set; }
    public string EstadoExtra { get; set; } = "ACT";
}
