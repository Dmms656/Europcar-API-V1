using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class MarcaVehiculoEntity : BaseEstadoEntity
{
    public int IdMarca { get; set; }
    public Guid MarcaGuid { get; set; }
    public string CodigoMarca { get; set; } = string.Empty;
    public string NombreMarca { get; set; } = string.Empty;
    public string? DescripcionMarca { get; set; }
    public string EstadoMarca { get; set; } = "ACT";

    // Navigation
    public ICollection<VehiculoEntity> Vehiculos { get; set; } = new List<VehiculoEntity>();
}
