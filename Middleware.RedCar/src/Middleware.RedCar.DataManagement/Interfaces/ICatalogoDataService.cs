using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataManagement.Models.Catalogo;

namespace Middleware.RedCar.DataManagement.Interfaces;

/// <summary>
/// Servicio interno que orquesta consultas a MS.Catalogo y devuelve
/// modelos de dominio del middleware (no DTOs del cable).
/// </summary>
public interface ICatalogoDataService
{
    Task<(IReadOnlyList<VehiculoDataModel> Items, int Total)> BuscarVehiculosAsync(VehiculoQuery query, CancellationToken ct = default);
    Task<VehiculoDataModel?> GetVehiculoAsync(int idVehiculo, CancellationToken ct = default);
    Task<(IReadOnlyList<CategoriaDataModel> Items, int Total)> ListCategoriasAsync(int page, int limit, CancellationToken ct = default);
    Task<(IReadOnlyList<ExtraDataModel> Items, int Total)> ListExtrasAsync(int page, int limit, CancellationToken ct = default);
    Task<IReadOnlyList<ExtraDataModel>> GetExtrasByIdsAsync(IEnumerable<int> idExtras, CancellationToken ct = default);
}
