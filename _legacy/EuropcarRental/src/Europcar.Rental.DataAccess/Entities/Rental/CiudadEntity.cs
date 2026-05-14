using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class CiudadEntity : BaseEstadoEntity
{
    public int IdCiudad { get; set; }
    public Guid CiudadGuid { get; set; }
    public int IdPais { get; set; }
    public string NombreCiudad { get; set; } = string.Empty;
    public string EstadoCiudad { get; set; } = "ACT";

    // Navigation
    public PaisEntity Pais { get; set; } = null!;
    public ICollection<LocalizacionEntity> Localizaciones { get; set; } = new List<LocalizacionEntity>();
}
