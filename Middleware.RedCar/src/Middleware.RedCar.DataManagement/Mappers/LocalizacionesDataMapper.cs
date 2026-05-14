using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataManagement.Models.Localizaciones;

namespace Middleware.RedCar.DataManagement.Mappers;

internal static class LocalizacionesDataMapper
{
    public static LocalizacionDataModel ToData(LocalizacionDto src) => new(
        src.IdLocalizacion,
        src.Codigo,
        src.Nombre,
        src.Direccion,
        src.Telefono,
        src.Correo,
        src.HorarioAtencion,
        src.ZonaHoraria,
        src.Estado,
        src.IdCiudad,
        src.CiudadNombre);
}
