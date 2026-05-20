using System.Globalization;
using RedCar.Reservas.Api.Contracts;
using RedCar.Shared.Protos.Reservas;

namespace RedCar.Reservas.Api.Mapping;

internal static class ReservasWriteMapper
{
    public static CrearReservaRequest ToProto(CrearReservaRestRequest src)
    {
        var req = new CrearReservaRequest
        {
            IdVehiculo = src.IdVehiculo,
            IdLocalizacionRecogida = src.IdLocalizacionRecogida,
            IdLocalizacionDevolucion = src.IdLocalizacionDevolucion,
            FechaInicio = src.FechaInicio,
            FechaFin = src.FechaFin,
            HoraInicio = src.HoraInicio,
            HoraFin = src.HoraFin,
            Observaciones = src.Observaciones ?? string.Empty,
            OrigenCanalReserva = src.OrigenCanalReserva ?? "BOOKING",
            IdCliente = src.IdCliente,
            Cliente = new ClienteDto
            {
                Nombres = src.Cliente.Nombres,
                Apellidos = src.Cliente.Apellidos,
                TipoIdentificacion = src.Cliente.TipoIdentificacion,
                NumeroIdentificacion = src.Cliente.NumeroIdentificacion,
                Correo = src.Cliente.Correo,
                Telefono = src.Cliente.Telefono
            }
        };

        foreach (var c in src.Conductores)
        {
            req.Conductores.Add(new ConductorDto
            {
                IdConductor = c.IdConductor,
                Nombres = c.Nombres,
                Apellidos = c.Apellidos,
                TipoIdentificacion = c.TipoIdentificacion,
                NumeroIdentificacion = c.NumeroIdentificacion,
                FechaVencimientoLicencia = c.FechaVencimientoLicencia,
                EdadConductor = c.EdadConductor,
                Correo = c.Correo,
                Telefono = c.Telefono,
                EsPrincipal = c.EsPrincipal
            });
        }

        foreach (var e in src.Extras)
        {
            req.Extras.Add(new ExtraDto { IdExtra = e.IdExtra, Cantidad = e.Cantidad });
        }

        return req;
    }

    public static CrearReservaRestResponse ToRest(CrearReservaResponse src) => new()
    {
        CodigoReserva = src.CodigoReserva,
        EstadoReserva = src.EstadoReserva,
        FechaReservaUtc = DateTimeOffset.Parse(src.FechaReservaUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
        CantidadDias = src.CantidadDias,
        SubtotalVehiculo = (decimal)src.SubtotalVehiculo,
        SubtotalExtras = (decimal)src.SubtotalExtras,
        Subtotal = (decimal)src.Subtotal,
        Iva = (decimal)src.Iva,
        Total = (decimal)src.Total
    };

    public static CancelarReservaRequest ToProto(string codigoReserva, CancelarReservaRestRequest src) => new()
    {
        CodigoReserva = codigoReserva,
        MotivoCancelacion = src.MotivoCancelacion,
        UsuarioCancelacion = src.UsuarioCancelacion ?? "BOOKING"
    };

    public static CancelarReservaRestResponse ToRest(CancelarReservaResponse src) => new()
    {
        CodigoReserva = src.CodigoReserva,
        EstadoReserva = src.EstadoReserva,
        FechaCancelacionUtc = DateTimeOffset.Parse(src.FechaCancelacionUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
    };
}
