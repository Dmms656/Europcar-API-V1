namespace RedCar.Catalogo.DataAccess.Entities;

public sealed class MarcaVehiculo
{
    public int IdMarca { get; set; }
    public Guid MarcaGuid { get; set; }
    public string CodigoMarca { get; set; } = string.Empty;
    public string NombreMarca { get; set; } = string.Empty;
    public string? DescripcionMarca { get; set; }
    public string EstadoMarca { get; set; } = "ACT";
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
