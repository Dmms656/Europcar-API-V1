using Middleware.RedCar.DataManagement.Models.Localizaciones;

namespace Middleware.RedCar.DataManagement.Interfaces;

public interface ILocalizacionesDataService
{
    Task<(IReadOnlyList<LocalizacionDataModel> Items, int Total)> ListAsync(int? idCiudad, int page, int limit, CancellationToken ct = default);
    Task<LocalizacionDataModel?> GetAsync(int idLocalizacion, CancellationToken ct = default);
}
