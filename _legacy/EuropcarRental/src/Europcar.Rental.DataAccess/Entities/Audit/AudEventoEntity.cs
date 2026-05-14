using Europcar.Rental.DataAccess.Entities.Common;

namespace Europcar.Rental.DataAccess.Entities.Audit;

public class AudEventoEntity
{
    public long IdAudEvento { get; set; }
    public Guid AudEventoGuid { get; set; }
    public string EsquemaAfectado { get; set; } = string.Empty;
    public string TablaAfectada { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public string? IdRegistroAfectado { get; set; }
    public string? DatosAnteriores { get; set; }
    public string? DatosNuevos { get; set; }
    public string? UsuarioApp { get; set; }
    public string? LoginBd { get; set; }
    public string? IpOrigen { get; set; }
    public string OrigenEvento { get; set; } = string.Empty;
    public DateTimeOffset FechaEventoUtc { get; set; }
    public long RowVersion { get; set; } = 1;
}
