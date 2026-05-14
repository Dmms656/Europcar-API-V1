namespace Middleware.RedCar.DataManagement.Models.Localizaciones;

public sealed record LocalizacionDataModel(
    int IdLocalizacion,
    string Codigo,
    string Nombre,
    string Direccion,
    string Telefono,
    string Correo,
    string HorarioAtencion,
    string ZonaHoraria,
    string Estado,
    int IdCiudad,
    string CiudadNombre);
