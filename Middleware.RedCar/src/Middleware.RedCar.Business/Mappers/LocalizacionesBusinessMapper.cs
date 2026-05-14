using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.DataManagement.Models.Localizaciones;

namespace Middleware.RedCar.Business.Mappers;

public static class LocalizacionesBusinessMapper
{
    public static LocalizacionBookingResponse ToBooking(LocalizacionDataModel l) =>
        new()
        {
            IdLocalizacion = l.IdLocalizacion,
            Codigo = l.Codigo,
            Nombre = l.Nombre,
            Direccion = l.Direccion,
            Telefono = l.Telefono,
            Correo = l.Correo,
            HorarioAtencion = l.HorarioAtencion,
            ZonaHoraria = l.ZonaHoraria,
            Estado = l.Estado,
            Ciudad = new CiudadResumen { IdCiudad = l.IdCiudad, Nombre = l.CiudadNombre },
            _Links = new Dictionary<string, LinkHref>
            {
                ["self"] = new() { Href = $"/api/v2/booking/localizaciones/{l.IdLocalizacion}" }
            }
        };
}
