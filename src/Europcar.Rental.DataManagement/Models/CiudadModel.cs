namespace Europcar.Rental.DataManagement.Models;

public class CiudadModel
{
    public int IdCiudad { get; set; }
    public Guid CiudadGuid { get; set; }
    public int IdPais { get; set; }
    public string NombreCiudad { get; set; } = string.Empty;
    public string? NombrePais { get; set; }
    public string EstadoCiudad { get; set; } = "ACT";
}
