using Middleware.RedCar.Business.DTOs.Reservas;
using Middleware.RedCar.Business.Exceptions;

namespace Middleware.RedCar.Business.Compatibility;

public static class LegacyCrearReservaMapper
{
    private static readonly DateOnly LicenciaPlaceholder = new(2035, 12, 31);

    /// <summary>
    /// Convierte el payload del monolito/frontend al DTO del contrato V2.
    /// </summary>
    public static CrearReservaBookingRequest ToContract(LegacyCrearReservaPayload src)
    {
        if (!int.TryParse(src.IdVehiculo.Trim(), out var idVeh))
            throw new ValidationException(new[]
            {
                new ValidationFailure("idVehiculo",
                    "Por ahora el middleware solo acepta idVehiculo numérico. " +
                    "Si envías codigoInterno alfanumérico, hace falta que MS.Catalogo exponga resolución por código.")
            });

        var idDev = src.IdLocalizacionDevolucion ?? src.IdLocalizacionEntrega
            ?? throw new ValidationException(new[]
            {
                new ValidationFailure("idLocalizacionDevolucion",
                    "Se requiere idLocalizacionDevolucion o idLocalizacionEntrega.")
            });

        var horaIni = src.HoraInicio ?? new TimeOnly(9, 0, 0);
        var horaFin = src.HoraFin ?? new TimeOnly(9, 0, 0);

        var conductores = BuildConductores(src);

        return new CrearReservaBookingRequest
        {
            IdVehiculo = idVeh,
            IdLocalizacionRecogida = src.IdLocalizacionRecogida,
            IdLocalizacionDevolucion = idDev,
            FechaInicio = src.FechaInicio,
            FechaFin = src.FechaFin,
            HoraInicio = horaIni,
            HoraFin = horaFin,
            Observaciones = src.Observaciones,
            Cliente = NormalizeCliente(src.Cliente),
            Conductores = conductores,
            Extras = src.Extras ?? new List<ExtraReservaRequest>()
        };
    }

    private static List<ConductorReservaRequest> BuildConductores(LegacyCrearReservaPayload src)
    {
        if (src.Conductores is { Count: > 0 })
        {
            return src.Conductores.Select(c => MapConductor(c, c.EsPrincipal)).ToList();
        }

        if (src.ConductorPrincipal is null)
            throw new ValidationException(new[]
                { new ValidationFailure("conductorPrincipal", "conductorPrincipal es obligatorio.") });

        var list = new List<ConductorReservaRequest> { MapConductor(src.ConductorPrincipal, true) };
        if (src.ConductorSecundario is not null &&
            !string.IsNullOrWhiteSpace(src.ConductorSecundario.NumeroIdentificacion))
        {
            list.Add(MapConductor(src.ConductorSecundario, false));
        }

        return list;
    }

    private static ConductorReservaRequest MapConductor(ConductorLegacyPayload c, bool esPrincipal) =>
        new()
        {
            Nombres = c.Nombres,
            Apellidos = c.Apellidos,
            TipoIdentificacion = MapTipoId(c.TipoIdentificacion),
            NumeroIdentificacion = c.NumeroIdentificacion,
            FechaVencimientoLicencia = c.FechaVencimientoLicencia ?? LicenciaPlaceholder,
            EdadConductor = c.EdadConductor <= 0 ? 25 : c.EdadConductor,
            Correo = c.Correo,
            Telefono = c.Telefono,
            EsPrincipal = esPrincipal
        };

    private static ClienteReservaRequest NormalizeCliente(ClienteReservaRequest c) => new()
    {
        Nombres = c.Nombres,
        Apellidos = c.Apellidos,
        TipoIdentificacion = MapTipoId(c.TipoIdentificacion),
        NumeroIdentificacion = c.NumeroIdentificacion,
        Correo = c.Correo,
        Telefono = c.Telefono
    };

    private static string MapTipoId(string? t)
    {
        if (string.IsNullOrWhiteSpace(t)) return "CEDULA";
        return t.Trim().ToUpperInvariant() switch
        {
            "CED" => "CEDULA",
            "PAS" => "PASAPORTE",
            "RUC" => "RUC",
            _ => t.Trim().ToUpperInvariant()
        };
    }
}
