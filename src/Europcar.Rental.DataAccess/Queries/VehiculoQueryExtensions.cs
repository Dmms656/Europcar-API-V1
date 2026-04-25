using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Queries;

/// <summary>
/// Extensiones de IQueryable para consultas complejas de vehículos.
/// Mantienen la lógica de filtrado fuera de los repositorios concretos.
/// </summary>
public static class VehiculoQueryExtensions
{
    public static IQueryable<VehiculoEntity> FiltrarDisponibles(this IQueryable<VehiculoEntity> query)
    {
        return query.Where(v => v.EstadoOperativo == "DISPONIBLE" && v.EstadoVehiculo == "ACT");
    }

    public static IQueryable<VehiculoEntity> FiltrarPorLocalizacion(this IQueryable<VehiculoEntity> query, int? localizacionId)
    {
        if (!localizacionId.HasValue) return query;
        return query.Where(v => v.LocalizacionActual == localizacionId.Value);
    }

    public static IQueryable<VehiculoEntity> FiltrarPorCategoria(this IQueryable<VehiculoEntity> query, int? categoriaId)
    {
        if (!categoriaId.HasValue) return query;
        return query.Where(v => v.IdCategoria == categoriaId.Value);
    }

    public static IQueryable<VehiculoEntity> FiltrarPorMarca(this IQueryable<VehiculoEntity> query, int? marcaId)
    {
        if (!marcaId.HasValue) return query;
        return query.Where(v => v.IdMarca == marcaId.Value);
    }

    public static IQueryable<VehiculoEntity> FiltrarPorTransmision(this IQueryable<VehiculoEntity> query, string? tipoTransmision)
    {
        if (string.IsNullOrWhiteSpace(tipoTransmision)) return query;
        return query.Where(v => v.TipoTransmision == tipoTransmision.ToUpper());
    }

    public static IQueryable<VehiculoEntity> FiltrarPorPrecio(this IQueryable<VehiculoEntity> query, decimal? precioMin, decimal? precioMax)
    {
        if (precioMin.HasValue)
            query = query.Where(v => v.PrecioBaseDia >= precioMin.Value);
        if (precioMax.HasValue)
            query = query.Where(v => v.PrecioBaseDia <= precioMax.Value);
        return query;
    }

    public static IQueryable<VehiculoEntity> FiltrarPorCapacidad(this IQueryable<VehiculoEntity> query, short? minPasajeros)
    {
        if (!minPasajeros.HasValue) return query;
        return query.Where(v => v.CapacidadPasajeros >= minPasajeros.Value);
    }

    public static IQueryable<VehiculoEntity> BuscarPorTexto(this IQueryable<VehiculoEntity> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        var term = search.ToLower();
        return query.Where(v =>
            v.ModeloVehiculo.ToLower().Contains(term) ||
            v.PlacaVehiculo.ToLower().Contains(term) ||
            v.ColorVehiculo.ToLower().Contains(term) ||
            v.CodigoInternoVehiculo.ToLower().Contains(term));
    }
}
