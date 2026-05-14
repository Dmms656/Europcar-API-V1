using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataManagement.Models.Clientes;

namespace Middleware.RedCar.DataManagement.Interfaces;

public interface IClientesDataService
{
    /// <summary>
    /// Upsert: si existe el cliente por (tipoIdentificacion, numeroIdentificacion)
    /// lo recupera; si no, lo crea.
    /// </summary>
    Task<ClienteDataModel> UpsertClienteAsync(ClienteUpsertRequest req, CancellationToken ct = default);

    /// <summary>
    /// Upsert masivo de conductores asociados al cliente. Se ejecuta en una sola
    /// llamada al MS para mantener la atomicidad por reserva.
    /// </summary>
    Task<IReadOnlyList<ConductorDataModel>> UpsertConductoresAsync(int idCliente, IReadOnlyList<ConductorUpsertRequest> conductores, CancellationToken ct = default);
}
