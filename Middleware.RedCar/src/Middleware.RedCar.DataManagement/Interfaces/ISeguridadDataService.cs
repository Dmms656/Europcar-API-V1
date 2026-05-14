namespace Middleware.RedCar.DataManagement.Interfaces;

public interface ISeguridadDataService
{
    Task<bool> PingAsync(CancellationToken ct = default);
}
