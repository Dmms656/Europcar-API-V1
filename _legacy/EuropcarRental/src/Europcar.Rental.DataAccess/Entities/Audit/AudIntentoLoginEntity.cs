namespace Europcar.Rental.DataAccess.Entities.Audit;

public class AudIntentoLoginEntity
{
    public long IdAudLogin { get; set; }
    public Guid AudLoginGuid { get; set; }
    public string UsernameIntentado { get; set; } = string.Empty;
    public string? CorreoIntentado { get; set; }
    public string Resultado { get; set; } = string.Empty;
    public string? Motivo { get; set; }
    public string? IpOrigen { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset FechaEventoUtc { get; set; }
    public long RowVersion { get; set; } = 1;
}
