using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataManagement.Models.Reservas;

namespace Middleware.RedCar.DataManagement.Mappers;

internal static class ReservasDataMapper
{
    public static ReservaDataModel ToData(ReservaDto src) => new()
    {
        CodigoReserva = src.CodigoReserva,
        EstadoReserva = src.EstadoReserva,
        OrigenCanalReserva = src.OrigenCanalReserva,
        FechaReservaUtc = src.FechaReservaUtc,
        FechaConfirmacionUtc = src.FechaConfirmacionUtc,
        FechaCancelacionUtc = src.FechaCancelacionUtc,
        MotivoCancelacion = src.MotivoCancelacion,
        Observaciones = src.Observaciones,
        Vehiculo = new ReservaVehiculoData(src.Vehiculo.IdVehiculo, src.Vehiculo.CodigoInterno, src.Vehiculo.Marca, src.Vehiculo.Modelo),
        LocalizacionRecogida = new ReservaLocalizacionData(src.LocalizacionRecogida.IdLocalizacion, src.LocalizacionRecogida.Nombre),
        LocalizacionDevolucion = new ReservaLocalizacionData(src.LocalizacionDevolucion.IdLocalizacion, src.LocalizacionDevolucion.Nombre),
        FechaInicio = src.FechaInicio,
        FechaFin = src.FechaFin,
        HoraInicio = src.HoraInicio,
        HoraFin = src.HoraFin,
        CantidadDias = src.CantidadDias,
        Cliente = new ReservaClienteData(src.Cliente.Nombres, src.Cliente.Apellidos, src.Cliente.TipoIdentificacion, src.Cliente.NumeroIdentificacion, src.Cliente.Correo, src.Cliente.Telefono),
        Conductores = src.Conductores.Select(c => new ReservaConductorData(c.Nombres, c.Apellidos, c.TipoIdentificacion, c.NumeroIdentificacion, c.EdadConductor, c.EsPrincipal)).ToList(),
        Extras = src.Extras.Select(e => new ReservaExtraData(e.IdExtra, e.Nombre, e.Cantidad, e.ValorUnitario, e.Subtotal)).ToList(),
        SubtotalVehiculo = src.SubtotalVehiculo,
        SubtotalExtras = src.SubtotalExtras,
        Subtotal = src.Subtotal,
        Iva = src.Iva,
        Total = src.Total
    };

    public static FacturaDataModel ToData(FacturaDto src) => new(
        src.NumeroFactura,
        src.CodigoReserva,
        src.FechaFacturaUtc,
        src.Subtotal,
        src.Iva,
        src.Total,
        src.Moneda,
        src.UrlPdf);
}
