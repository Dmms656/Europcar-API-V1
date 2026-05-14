using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.DataManagement.Models.Catalogo;

namespace Middleware.RedCar.Business.Mappers;

/// <summary>
/// Mapeo DataModel (interno) -> contrato publico Booking.
/// La logica de precios (subtotal, IVA, total) se calcula aqui porque
/// el orchestrator es el unico responsable de aplicarla por reserva.
/// </summary>
public static class CatalogoBusinessMapper
{
    public static VehiculoBookingResponse ToBooking(
        VehiculoDataModel v,
        DateTimeOffset fechaRecogida,
        DateTimeOffset fechaDevolucion,
        bool disponible,
        decimal ivaPorcentaje,
        string nombreLocalizacion,
        string direccionLocalizacion)
    {
        var dias = CalcularDias(fechaRecogida, fechaDevolucion);
        var subtotal = Math.Round(v.PrecioBaseDia * dias, 2);
        var iva = Math.Round(subtotal * ivaPorcentaje / 100m, 2);
        var total = Math.Round(subtotal + iva, 2);

        return new VehiculoBookingResponse
        {
            IdVehiculo = v.IdVehiculo,
            CodigoInterno = v.CodigoInterno,
            Marca = v.Marca,
            Modelo = v.Modelo,
            Anio = v.Anio,
            Color = v.Color,
            ImagenUrl = v.ImagenUrl,
            Transmision = v.Transmision,
            Combustible = v.Combustible,
            CapacidadPasajeros = v.CapacidadPasajeros,
            CapacidadMaletas = v.CapacidadMaletas,
            NumeroPuertas = v.NumeroPuertas,
            AireAcondicionado = v.AireAcondicionado,
            Estado = v.Estado,
            Localizacion = new LocalizacionResumen
            {
                IdLocalizacion = v.IdLocalizacion,
                Codigo = string.Empty,
                Nombre = nombreLocalizacion,
                Direccion = direccionLocalizacion
            },
            Disponibilidad = new DisponibilidadResumen
            {
                FechaRecogida = fechaRecogida,
                FechaDevolucion = fechaDevolucion,
                CantidadDias = dias,
                Disponible = disponible
            },
            Precio = new PrecioResumen
            {
                PrecioBaseDia = v.PrecioBaseDia,
                SubtotalVehiculo = subtotal,
                Iva = iva,
                Total = total
            },
            _Links = new Dictionary<string, LinkHref>
            {
                ["self"] = new() { Href = $"/api/v2/booking/vehiculos/{v.IdVehiculo}" },
                // Contrato (tabla Endpoint 3): GET .../reservas/{idVehiculo}/disponibilidad
                ["disponibilidad"] = new() { Href = $"/api/v2/booking/reservas/{v.IdVehiculo}/disponibilidad" }
            }
        };
    }

    public static VehiculoDetalleResponse ToDetalle(VehiculoDataModel v, string nombreLocalizacion, string codigoLocalizacion, string direccionLocalizacion) =>
        new()
        {
            IdVehiculo = v.IdVehiculo,
            CodigoInterno = v.CodigoInterno,
            Marca = new MarcaResumen { IdMarca = v.IdMarca, Nombre = v.Marca },
            Categoria = new CategoriaResumen { IdCategoria = v.IdCategoria, Codigo = v.CategoriaCodigo, Nombre = v.CategoriaNombre },
            Modelo = v.Modelo,
            Anio = v.Anio,
            Color = v.Color,
            ImagenUrl = v.ImagenUrl,
            Transmision = v.Transmision,
            Combustible = v.Combustible,
            CapacidadPasajeros = v.CapacidadPasajeros,
            CapacidadMaletas = v.CapacidadMaletas,
            NumeroPuertas = v.NumeroPuertas,
            AireAcondicionado = v.AireAcondicionado,
            Estado = v.Estado,
            Localizacion = new LocalizacionResumen
            {
                IdLocalizacion = v.IdLocalizacion,
                Codigo = codigoLocalizacion,
                Nombre = nombreLocalizacion,
                Direccion = direccionLocalizacion
            },
            _Links = new Dictionary<string, LinkHref>
            {
                ["self"] = new() { Href = $"/api/v2/booking/vehiculos/{v.IdVehiculo}" },
                // Contrato (tabla Endpoint 3): GET .../reservas/{idVehiculo}/disponibilidad
                ["disponibilidad"] = new() { Href = $"/api/v2/booking/reservas/{v.IdVehiculo}/disponibilidad" }
            }
        };

    public static CategoriaBookingResponse ToBooking(CategoriaDataModel c) =>
        new()
        {
            IdCategoria = c.IdCategoria,
            Codigo = c.Codigo,
            Nombre = c.Nombre,
            Descripcion = c.Descripcion,
            Estado = c.Estado
        };

    public static ExtraBookingResponse ToBooking(ExtraDataModel e) =>
        new()
        {
            IdExtra = e.IdExtra,
            Codigo = e.Codigo,
            Nombre = e.Nombre,
            Descripcion = e.Descripcion,
            ValorFijo = e.ValorFijo,
            Estado = e.Estado
        };

    /// <summary>
    /// Dias de la reserva = ceil((fechaDevolucion - fechaRecogida) / 24h).
    /// Minimo 1.
    /// </summary>
    public static int CalcularDias(DateTimeOffset desde, DateTimeOffset hasta)
    {
        if (hasta <= desde) return 1;
        var span = hasta - desde;
        return Math.Max(1, (int)Math.Ceiling(span.TotalHours / 24.0));
    }
}
