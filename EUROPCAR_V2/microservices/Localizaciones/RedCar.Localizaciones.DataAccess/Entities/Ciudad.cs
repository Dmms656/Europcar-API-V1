namespace RedCar.Localizaciones.DataAccess.Entities;

public sealed class Ciudad
{
    public int IdCiudad { get; set; }
    public string NombreCiudad { get; set; } = string.Empty;
    public string EstadoCiudad { get; set; } = "ACT";
    public bool EsEliminado { get; set; }
}
