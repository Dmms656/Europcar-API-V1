using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class LocalizacionExtraStockEntity : BaseEntity
{
    public int IdLocalizacionExtraStock { get; set; }
    public Guid StockGuid { get; set; }
    public int IdLocalizacion { get; set; }
    public int IdExtra { get; set; }
    public int StockDisponible { get; set; }
    public int StockReservado { get; set; }
    public string EstadoStock { get; set; } = "ACT";
    public string OrigenRegistro { get; set; } = string.Empty;

    // Navigation
    public LocalizacionEntity Localizacion { get; set; } = null!;
    public ExtraEntity Extra { get; set; } = null!;
}
