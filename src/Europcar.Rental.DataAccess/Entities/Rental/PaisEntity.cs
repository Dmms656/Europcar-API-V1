using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class PaisEntity : BaseEstadoEntity
{
    public int IdPais { get; set; }
    public Guid PaisGuid { get; set; }
    public string CodigoIso2 { get; set; } = string.Empty;
    public string NombrePais { get; set; } = string.Empty;
    public string EstadoPais { get; set; } = "ACT";

    // Navigation
    public ICollection<CiudadEntity> Ciudades { get; set; } = new List<CiudadEntity>();
}
