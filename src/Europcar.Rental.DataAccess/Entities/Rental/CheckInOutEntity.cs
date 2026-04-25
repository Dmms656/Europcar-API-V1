namespace Europcar.Rental.DataAccess.Entities.Rental;

public class CheckInOutEntity
{
    public int IdCheck { get; set; }
    public Guid CheckGuid { get; set; }
    public int IdContrato { get; set; }
    public string TipoCheck { get; set; } = string.Empty;
    public DateTimeOffset FechaHoraCheck { get; set; }
    public int Kilometraje { get; set; }
    public decimal NivelCombustible { get; set; }
    public bool Limpio { get; set; } = true;
    public string? Observaciones { get; set; }
    public decimal CargoCombustible { get; set; }
    public decimal CargoLimpieza { get; set; }
    public decimal CargoKmExtra { get; set; }
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public DateTimeOffset FechaRegistroUtc { get; set; } = DateTimeOffset.UtcNow;
    public long RowVersion { get; set; } = 1;

    // Navigation
    public ContratoEntity Contrato { get; set; } = null!;
}
