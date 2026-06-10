using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataManagement.Interfaces;
using Middleware.RedCar.DataManagement.Mappers;
using Middleware.RedCar.DataManagement.Models.Catalogo;

namespace Middleware.RedCar.DataManagement.Services;

public sealed class CatalogoDataService : ICatalogoDataService
{
    private readonly ICatalogoClient _client;

    public CatalogoDataService(ICatalogoClient client) => _client = client;

    public async Task<(IReadOnlyList<VehiculoDataModel> Items, int Total)> BuscarVehiculosAsync(VehiculoQuery query, CancellationToken ct = default)
    {
        var paged = await _client.BuscarVehiculosAsync(query, ct);
        if (paged is null) return (Array.Empty<VehiculoDataModel>(), 0);
        var items = paged.Items.Select(CatalogoDataMapper.ToData).ToList();
        return (items, paged.TotalElementos);
    }

    public async Task<VehiculoDataModel?> GetVehiculoAsync(int idVehiculo, CancellationToken ct = default)
    {
        var dto = await _client.GetVehiculoAsync(idVehiculo, ct);
        return dto is null ? null : CatalogoDataMapper.ToData(dto);
    }

    public async Task<(IReadOnlyList<CategoriaDataModel> Items, int Total)> ListCategoriasAsync(int page, int limit, CancellationToken ct = default)
    {
        var paged = await _client.ListCategoriasAsync(page, limit, ct);
        if (paged is null) return (Array.Empty<CategoriaDataModel>(), 0);
        var items = paged.Items.Select(CatalogoDataMapper.ToData).ToList();
        return (items, paged.TotalElementos);
    }

    public async Task<(IReadOnlyList<ExtraDataModel> Items, int Total)> ListExtrasAsync(int page, int limit, CancellationToken ct = default)
    {
        var paged = await _client.ListExtrasAsync(page, limit, ct);
        if (paged is null) return (Array.Empty<ExtraDataModel>(), 0);
        var items = paged.Items.Select(CatalogoDataMapper.ToData).ToList();
        return (items, paged.TotalElementos);
    }

    public async Task<IReadOnlyList<ExtraDataModel>> GetExtrasByIdsAsync(IEnumerable<int> idExtras, CancellationToken ct = default)
    {
        var dtos = await _client.GetExtrasByIdsAsync(idExtras, ct);
        return dtos is null ? Array.Empty<ExtraDataModel>() : dtos.Select(CatalogoDataMapper.ToData).ToList();
    }
}
