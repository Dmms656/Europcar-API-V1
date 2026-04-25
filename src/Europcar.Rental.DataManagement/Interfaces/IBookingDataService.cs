using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Interfaces;

/// <summary>
/// Servicio de datos especializado para las consultas del módulo Booking / OTA.
/// Expone extras con precio y localizaciones con ciudad Id.
/// </summary>
public interface IBookingDataService
{
    Task<IEnumerable<BookingExtraModel>> GetExtrasConPrecioAsync();
    Task<IEnumerable<BookingLocalizacionModel>> GetLocalizacionesConCiudadIdAsync();
    Task<BookingLocalizacionModel?> GetLocalizacionConCiudadIdAsync(int id);
}
