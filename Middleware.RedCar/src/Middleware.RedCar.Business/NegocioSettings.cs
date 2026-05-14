namespace Middleware.RedCar.Business;

/// <summary>
/// Parametros de negocio del middleware. Se enlaza a la seccion "Negocio" del config.
/// </summary>
public sealed class NegocioSettings
{
    public const string SectionName = "Negocio";

    /// <summary>Porcentaje de IVA aplicado a las reservas (ej: 15 = 15%).</summary>
    public decimal IvaPorcentaje { get; set; } = 15m;

    /// <summary>Canal por el que entran las reservas a traves del middleware. Por contrato, "BOOKING".</summary>
    public string OrigenCanalReserva { get; set; } = "BOOKING";
}
