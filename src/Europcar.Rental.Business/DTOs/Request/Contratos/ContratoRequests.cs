namespace Europcar.Rental.Business.DTOs.Request.Contratos;

public class CrearContratoRequest
{
    public int IdReserva { get; set; }
    public int KilometrajeSalida { get; set; }
    public decimal NivelCombustibleSalida { get; set; }
    public string? Observaciones { get; set; }
}

public class CheckOutRequest
{
    public int IdContrato { get; set; }
    public int Kilometraje { get; set; }
    public decimal NivelCombustible { get; set; }
    public bool Limpio { get; set; } = true;
    public string? Observaciones { get; set; }
}

public class CheckInRequest
{
    public int IdContrato { get; set; }
    public int Kilometraje { get; set; }
    public decimal NivelCombustible { get; set; }
    public bool Limpio { get; set; } = true;
    public string? Observaciones { get; set; }
}
