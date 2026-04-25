using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Mappers;

public static class VehiculoMapper
{
    public static VehiculoModel ToModel(this VehiculoEntity entity) => new()
    {
        IdVehiculo = entity.IdVehiculo,
        VehiculoGuid = entity.VehiculoGuid,
        CodigoInterno = entity.CodigoInternoVehiculo,
        Placa = entity.PlacaVehiculo,
        Marca = entity.Marca?.NombreMarca ?? string.Empty,
        Categoria = entity.Categoria?.NombreCategoria ?? string.Empty,
        Modelo = entity.ModeloVehiculo,
        AnioFabricacion = entity.AnioFabricacion,
        Color = entity.ColorVehiculo,
        TipoCombustible = entity.TipoCombustible,
        TipoTransmision = entity.TipoTransmision,
        CapacidadPasajeros = entity.CapacidadPasajeros,
        CapacidadMaletas = entity.CapacidadMaletas,
        NumeroPuertas = entity.NumeroPuertas,
        PrecioBaseDia = entity.PrecioBaseDia,
        KilometrajeActual = entity.KilometrajeActual,
        AireAcondicionado = entity.AireAcondicionado,
        EstadoOperativo = entity.EstadoOperativo,
        ImagenUrl = entity.ImagenReferencialUrl,
        IdLocalizacion = entity.LocalizacionActual,
        NombreLocalizacion = entity.Localizacion?.NombreLocalizacion ?? string.Empty
    };
}
