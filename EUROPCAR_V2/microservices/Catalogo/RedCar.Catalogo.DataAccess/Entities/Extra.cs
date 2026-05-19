namespace RedCar.Catalogo.DataAccess.Entities;

public sealed class Extra
{
    public int IdExtra { get; set; }
    public Guid ExtraGuid { get; set; }
    public string CodigoExtra { get; set; } = string.Empty;
    public string NombreExtra { get; set; } = string.Empty;
    public string DescripcionExtra { get; set; } = string.Empty;
    public string TipoExtra { get; set; } = "SERVICIO";
    public bool RequiereStock { get; set; }
    public decimal ValorFijo { get; set; }
    public string EstadoExtra { get; set; } = "ACT";
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
