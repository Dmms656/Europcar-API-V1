using Europcar.Rental.Business.DTOs.Request.Localizaciones;
using Europcar.Rental.Business.DTOs.Response.Catalogos;

namespace Europcar.Rental.Business.Interfaces;

/// <summary>
/// Servicio de gestión administrativa de localizaciones (sucursales).
/// Pensado para ADMIN y AGENTE_POS.
/// </summary>
public interface ILocalizacionService
{
    Task<IEnumerable<LocalizacionResponse>> GetAllAsync(bool soloActivas = false);
    Task<LocalizacionResponse> GetByIdAsync(int id);
    Task<LocalizacionResponse> CreateAsync(CrearLocalizacionRequest request, string usuario);
    Task<LocalizacionResponse> UpdateAsync(int id, ActualizarLocalizacionRequest request, string usuario);
    Task CambiarEstadoAsync(int id, CambiarEstadoLocalizacionRequest request, string usuario);
    Task DeleteAsync(int id, string usuario);
    Task<IEnumerable<CiudadResponse>> GetCiudadesAsync();
}
