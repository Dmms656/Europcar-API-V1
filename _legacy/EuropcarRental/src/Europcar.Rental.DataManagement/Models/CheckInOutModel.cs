namespace Europcar.Rental.DataManagement.Models;

public class CheckInOutModel
{
    public int IdCheck { get; set; }
    public Guid CheckGuid { get; set; }
    public int IdContrato { get; set; }
    public string TipoCheck { get; set; } = string.Empty;
    public DateTimeOffset FechaHoraCheck { get; set; }
    public int Kilometraje { get; set; }
    public decimal NivelCombustible { get; set; }
    public bool Limpio { get; set; }
    public string? Observaciones { get; set; }
    public decimal CargoCombustible { get; set; }
    public decimal CargoLimpieza { get; set; }
    public decimal CargoKmExtra { get; set; }
}
