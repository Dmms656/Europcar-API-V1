namespace Middleware.RedCar.Api.Controllers.V1;

public sealed class LegacyCambiarEstadoBody
{
    public string Estado { get; set; } = "ACT";
    public string? Motivo { get; set; }
}
