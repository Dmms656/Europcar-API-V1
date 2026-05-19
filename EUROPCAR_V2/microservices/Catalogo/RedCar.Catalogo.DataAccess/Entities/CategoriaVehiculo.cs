namespace RedCar.Catalogo.DataAccess.Entities;

public sealed class CategoriaVehiculo
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
    public bool EsEliminado { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificadoDesdeIp { get; set; }
    public DateTimeOffset? FechaInhabilitacionUtc { get; set; }
    public string? MotivoInhabilitacion { get; set; }
    public string OrigenRegistro { get; set; } = string.Empty;
    public long RowVersion { get; set; }
}
