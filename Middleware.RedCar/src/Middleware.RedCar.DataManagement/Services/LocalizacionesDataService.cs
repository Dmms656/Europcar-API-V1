using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataManagement.Interfaces;
using Middleware.RedCar.DataManagement.Mappers;
using Middleware.RedCar.DataManagement.Models.Localizaciones;

namespace Middleware.RedCar.DataManagement.Services;

public sealed class LocalizacionesDataService : ILocalizacionesDataService
{
    private readonly ILocalizacionesClient _client;

    public LocalizacionesDataService(ILocalizacionesClient client)
    {
        _client = client;
    }

    public async Task<(IReadOnlyList<LocalizacionDataModel> Items, int Total)> ListAsync(int? idCiudad, int page, int limit, CancellationToken ct = default)
    {
        var paged = await _client.ListLocalizacionesAsync(idCiudad, page, limit, ct);
        if (paged is null) return (Array.Empty<LocalizacionDataModel>(), 0);
        var items = paged.Items.Select(LocalizacionesDataMapper.ToData).ToList();
        return (items, paged.TotalElementos);
    }

    public async Task<LocalizacionDataModel?> GetAsync(int idLocalizacion, CancellationToken ct = default)
    {
        var dto = await _client.GetLocalizacionAsync(idLocalizacion, ct);
        return dto is null ? null : LocalizacionesDataMapper.ToData(dto);
    }
}
