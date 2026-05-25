using Middleware.RedCar.Business.DTOs.Booking;

namespace Middleware.RedCar.Api.Compatibility;

/// <summary>
/// Aplana y renombra campos para compatibilidad con el frontend React que armó
/// contratos contra el monolito (precioBaseDia en raíz, id en localizaciones, etc.).
/// </summary>
public static class LegacyV1DtoMapper
{
    public static object ToVehiculoListaItem(VehiculoBookingResponse v) => new
    {
        idVehiculo = v.IdVehiculo,
        codigoInterno = v.CodigoInterno,
        marca = v.Marca,
        modelo = v.Modelo,
        anio = v.Anio,
        color = v.Color,
        imagenUrl = v.ImagenUrl,
        transmision = v.Transmision,
        combustible = v.Combustible,
        capacidadPasajeros = v.CapacidadPasajeros,
        capacidadMaletas = v.CapacidadMaletas,
        numeroPuertas = v.NumeroPuertas,
        aireAcondicionado = v.AireAcondicionado,
        estado = v.Estado,
        localizacion = new
        {
            id = v.Localizacion.IdLocalizacion,
            idLocalizacion = v.Localizacion.IdLocalizacion,
            nombre = v.Localizacion.Nombre,
            nombreLocalizacion = v.Localizacion.Nombre,
            direccion = v.Localizacion.Direccion
        },
        disponibilidad = v.Disponibilidad,
        precio = v.Precio,
        precioBaseDia = v.Precio.PrecioBaseDia,
        precioDia = v.Precio.PrecioBaseDia,
        _links = v._Links
    };

    public static object ToVehiculoDetalle(VehiculoDetalleResponse v) => new
    {
        idVehiculo = v.IdVehiculo,
        codigoInterno = v.CodigoInterno,
        marca = v.Marca,
        categoria = v.Categoria,
        modelo = v.Modelo,
        anio = v.Anio,
        color = v.Color,
        imagenUrl = v.ImagenUrl,
        transmision = v.Transmision,
        combustible = v.Combustible,
        capacidadPasajeros = v.CapacidadPasajeros,
        capacidadMaletas = v.CapacidadMaletas,
        numeroPuertas = v.NumeroPuertas,
        aireAcondicionado = v.AireAcondicionado,
        estado = v.Estado,
        localizacion = v.Localizacion,
        precio = v.Precio,
        precioBaseDia = v.Precio.PrecioBaseDia,
        precioDia = v.Precio.PrecioBaseDia,
        _links = v._Links
    };

    public static object ToLocalizacionListItem(LocalizacionBookingResponse l) => new
    {
        id = l.IdLocalizacion,
        idLocalizacion = l.IdLocalizacion,
        codigo = l.Codigo,
        nombre = l.Nombre,
        nombreLocalizacion = l.Nombre,
        direccion = l.Direccion,
        telefono = l.Telefono,
        correo = l.Correo,
        horarioAtencion = l.HorarioAtencion,
        zonaHoraria = l.ZonaHoraria,
        estado = l.Estado,
        idCiudad = l.Ciudad.IdCiudad,
        ciudad = new { id = l.Ciudad.IdCiudad, nombre = l.Ciudad.Nombre },
        _links = l._Links
    };

    public static object ToCategoriaItem(CategoriaBookingResponse c) => new
    {
        id = c.IdCategoria,
        idCategoria = c.IdCategoria,
        codigo = c.Codigo,
        nombre = c.Nombre,
        nombreCategoria = c.Nombre,
        descripcion = c.Descripcion,
        estado = c.Estado
    };

    public static object ToExtraItem(ExtraBookingResponse e) => new
    {
        id = e.IdExtra,
        idExtra = e.IdExtra,
        codigo = e.Codigo,
        nombre = e.Nombre,
        descripcion = e.Descripcion,
        valorFijo = e.ValorFijo,
        precio = e.ValorFijo,
        estado = e.Estado
    };
}
