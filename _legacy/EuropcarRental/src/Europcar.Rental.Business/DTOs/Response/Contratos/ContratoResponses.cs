namespace Europcar.Rental.Business.DTOs.Response.Contratos;

public class ContratoResponse
{
    public int IdContrato { get; set; }
    public Guid ContratoGuid { get; set; }
    public string NumeroContrato { get; set; } = string.Empty;
    public string EstadoContrato { get; set; } = string.Empty;
    public DateTimeOffset FechaHoraSalida { get; set; }
    public DateTimeOffset FechaHoraPrevistaDevolucion { get; set; }
    public int KilometrajeSalida { get; set; }
    public decimal NivelCombustibleSalida { get; set; }
    public string? NombreCliente { get; set; }
    public string? PlacaVehiculo { get; set; }
    public string? CodigoReserva { get; set; }
    public string? ObservacionesContrato { get; set; }
}

public class CheckInOutResponse
{
    public int IdCheck { get; set; }
    public Guid CheckGuid { get; set; }
    public string TipoCheck { get; set; } = string.Empty;
    public DateTimeOffset FechaHoraCheck { get; set; }
    public int Kilometraje { get; set; }
    public decimal NivelCombustible { get; set; }
    public bool Limpio { get; set; }
    public decimal CargoCombustible { get; set; }
    public decimal CargoLimpieza { get; set; }
    public decimal CargoKmExtra { get; set; }
    public decimal TotalCargosAdicionales => CargoCombustible + CargoLimpieza + CargoKmExtra;
    public string? Observaciones { get; set; }
}
