using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataManagement.Interfaces;

namespace Middleware.RedCar.DataManagement.Services;

public sealed class SeguridadDataService : ISeguridadDataService
{
    private readonly ISeguridadClient _client;

    public SeguridadDataService(ISeguridadClient client)
    {
        _client = client;
    }

    public Task<bool> PingAsync(CancellationToken ct = default) => _client.PingAsync(ct);
}
