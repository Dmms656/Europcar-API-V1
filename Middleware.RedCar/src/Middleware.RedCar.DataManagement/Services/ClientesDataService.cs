using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataManagement.Interfaces;
using Middleware.RedCar.DataManagement.Mappers;
using Middleware.RedCar.DataManagement.Models.Clientes;

namespace Middleware.RedCar.DataManagement.Services;

public sealed class ClientesDataService : IClientesDataService
{
    private readonly IClientesClient _client;

    public ClientesDataService(IClientesClient client)
    {
        _client = client;
    }

    public async Task<ClienteDataModel> UpsertClienteAsync(ClienteUpsertRequest req, CancellationToken ct = default)
    {
        var res = await _client.UpsertClienteAsync(req, ct)
            ?? throw new InvalidOperationException("MS.Clientes no devolvio resultado al hacer upsert de cliente.");
        return ClientesDataMapper.ToData(req, res);
    }

    public async Task<IReadOnlyList<ConductorDataModel>> UpsertConductoresAsync(int idCliente, IReadOnlyList<ConductorUpsertRequest> conductores, CancellationToken ct = default)
    {
        var res = await _client.UpsertConductoresAsync(idCliente, conductores, ct)
            ?? throw new InvalidOperationException("MS.Clientes no devolvio resultado al hacer upsert de conductores.");

        var byId = res.ToDictionary(r => r.NumeroIdentificacion, StringComparer.OrdinalIgnoreCase);
        var output = new List<ConductorDataModel>(conductores.Count);
        foreach (var c in conductores)
        {
            if (!byId.TryGetValue(c.NumeroIdentificacion, out var match))
            {
                throw new InvalidOperationException($"MS.Clientes no devolvio el conductor {c.NumeroIdentificacion}.");
            }
            output.Add(ClientesDataMapper.ToData(idCliente, c, match));
        }
        return output;
    }
}
