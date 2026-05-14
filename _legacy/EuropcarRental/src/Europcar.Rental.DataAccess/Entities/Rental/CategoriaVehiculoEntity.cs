using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Rental;

public class CategoriaVehiculoEntity : BaseEstadoEntity
{
    public int IdCategoria { get; set; }
    public Guid CategoriaGuid { get; set; }
    public string CodigoCategoria { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public string? DescripcionCategoria { get; set; }
    public bool KilometrajeIlimitado { get; set; } = true;
    public int? LimiteKmDia { get; set; }
    public decimal? CargoKmExcedente { get; set; }
    public string EstadoCategoria { get; set; } = "ACT";

    // Navigation
    public ICollection<VehiculoEntity> Vehiculos { get; set; } = new List<VehiculoEntity>();
}
