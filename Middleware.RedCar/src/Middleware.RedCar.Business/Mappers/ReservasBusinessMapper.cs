using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.Business.DTOs.Reservas;
using Middleware.RedCar.DataAccess.GrpcClients.Interfaces;
using Middleware.RedCar.DataManagement.Models.Reservas;

namespace Middleware.RedCar.Business.Mappers;

public static class ReservasBusinessMapper
{
    /// <summary>
    /// Construye el response del POST /reservas (Endpoint 8) a partir del resultado
    /// del gRPC + datos de vehiculo ya conocidos por el orquestador.
    /// </summary>
    public static CrearReservaBookingResponse ToBooking(
        CrearReservaGrpcResult grpc,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        VehiculoReservaResumen vehiculo) =>
        new()
        {
            CodigoReserva = grpc.CodigoReserva,
            EstadoReserva = grpc.EstadoReserva,
            FechaReservaUtc = grpc.FechaReservaUtc,
            Vehiculo = vehiculo,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            CantidadDias = grpc.CantidadDias,
            SubtotalVehiculo = grpc.SubtotalVehiculo,
            SubtotalExtras = grpc.SubtotalExtras,
            Subtotal = grpc.Subtotal,
            Iva = grpc.Iva,
            Total = grpc.Total,
            _Links = new Dictionary<string, LinkHref>
            {
                ["self"] = new() { Href = $"/api/v2/booking/reservas/{grpc.CodigoReserva}" },
                ["cancelar"] = new() { Href = $"/api/v2/booking/reservas/{grpc.CodigoReserva}/cancelar" }
            }
        };

    public static ReservaBookingResponse ToBooking(ReservaDataModel src) =>
        new()
        {
            CodigoReserva = src.CodigoReserva,
            EstadoReserva = src.EstadoReserva,
            OrigenCanalReserva = src.OrigenCanalReserva,
            FechaReservaUtc = src.FechaReservaUtc,
            FechaConfirmacionUtc = src.FechaConfirmacionUtc,
            FechaCancelacionUtc = src.FechaCancelacionUtc,
            MotivoCancelacion = src.MotivoCancelacion,
            Observaciones = src.Observaciones,
            Vehiculo = new VehiculoReservaResumen
            {
                IdVehiculo = src.Vehiculo.IdVehiculo,
                CodigoInterno = src.Vehiculo.CodigoInterno,
                Marca = src.Vehiculo.Marca,
                Modelo = src.Vehiculo.Modelo
            },
            LocalizacionRecogida = new LocalizacionResumenReserva
            {
                IdLocalizacion = src.LocalizacionRecogida.IdLocalizacion,
                Nombre = src.LocalizacionRecogida.Nombre
            },
            LocalizacionDevolucion = new LocalizacionResumenReserva
            {
                IdLocalizacion = src.LocalizacionDevolucion.IdLocalizacion,
                Nombre = src.LocalizacionDevolucion.Nombre
            },
            FechaInicio = src.FechaInicio,
            FechaFin = src.FechaFin,
            HoraInicio = src.HoraInicio,
            HoraFin = src.HoraFin,
            CantidadDias = src.CantidadDias,
            Cliente = new ClienteReservaResponse
            {
                Nombres = src.Cliente.Nombres,
                Apellidos = src.Cliente.Apellidos,
                TipoIdentificacion = src.Cliente.TipoIdentificacion,
                NumeroIdentificacion = src.Cliente.NumeroIdentificacion,
                Correo = src.Cliente.Correo,
                Telefono = src.Cliente.Telefono
            },
            Conductores = src.Conductores.Select(c => new ConductorReservaResponse
            {
                Nombres = c.Nombres,
                Apellidos = c.Apellidos,
                TipoIdentificacion = c.TipoIdentificacion,
                NumeroIdentificacion = c.NumeroIdentificacion,
                EdadConductor = c.EdadConductor,
                EsPrincipal = c.EsPrincipal
            }).ToList(),
            Extras = src.Extras.Select(e => new ExtraReservaResponse
            {
                IdExtra = e.IdExtra,
                Nombre = e.Nombre,
                Cantidad = e.Cantidad,
                ValorUnitario = e.ValorUnitario,
                Subtotal = e.Subtotal
            }).ToList(),
            SubtotalVehiculo = src.SubtotalVehiculo,
            SubtotalExtras = src.SubtotalExtras,
            Subtotal = src.Subtotal,
            Iva = src.Iva,
            Total = src.Total,
            _Links = new Dictionary<string, LinkHref>
            {
                ["self"] = new() { Href = $"/api/v2/booking/reservas/{src.CodigoReserva}" },
                ["factura"] = new() { Href = $"/api/v2/booking/reservas/{src.CodigoReserva}/factura" },
                ["cancelar"] = new() { Href = $"/api/v2/booking/reservas/{src.CodigoReserva}/cancelar" }
            }
        };

    public static FacturaBookingResponse ToBooking(FacturaDataModel src) =>
        new()
        {
            NumeroFactura = src.NumeroFactura,
            CodigoReserva = src.CodigoReserva,
            FechaFacturaUtc = src.FechaFacturaUtc,
            Subtotal = src.Subtotal,
            Iva = src.Iva,
            Total = src.Total,
            Moneda = src.Moneda,
            UrlPdf = src.UrlPdf
        };
}
