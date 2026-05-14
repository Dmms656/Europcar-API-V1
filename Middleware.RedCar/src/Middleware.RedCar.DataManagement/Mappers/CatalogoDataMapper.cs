using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataManagement.Models.Catalogo;

namespace Middleware.RedCar.DataManagement.Mappers;

internal static class CatalogoDataMapper
{
    public static VehiculoDataModel ToData(VehiculoCatalogoDto src) => new()
    {
        IdVehiculo = src.IdVehiculo,
        CodigoInterno = src.CodigoInterno,
        IdMarca = src.IdMarca,
        Marca = src.Marca,
        IdCategoria = src.IdCategoria,
        CategoriaCodigo = src.CategoriaCodigo,
        CategoriaNombre = src.CategoriaNombre,
        Modelo = src.Modelo,
        Anio = src.Anio,
        Color = src.Color,
        ImagenUrl = src.ImagenUrl,
        Transmision = src.Transmision,
        Combustible = src.Combustible,
        CapacidadPasajeros = src.CapacidadPasajeros,
        CapacidadMaletas = src.CapacidadMaletas,
        NumeroPuertas = src.NumeroPuertas,
        AireAcondicionado = src.AireAcondicionado,
        Estado = src.Estado,
        IdLocalizacion = src.IdLocalizacion,
        PrecioBaseDia = src.PrecioBaseDia
    };

    public static CategoriaDataModel ToData(CategoriaDto src) =>
        new(src.IdCategoria, src.Codigo, src.Nombre, src.Descripcion, src.Estado);

    public static ExtraDataModel ToData(ExtraDto src) =>
        new(src.IdExtra, src.Codigo, src.Nombre, src.Descripcion, src.ValorFijo, src.Estado);
}
