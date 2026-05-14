using Middleware.RedCar.Business.DTOs.Reservas;
using Middleware.RedCar.DataAccess.Clients.Interfaces;

namespace Middleware.RedCar.Business.Mappers;

public static class ClientesBusinessMapper
{
    public static ClienteUpsertRequest ToUpsert(ClienteReservaRequest src) => new(
        Nombres: src.Nombres,
        Apellidos: src.Apellidos,
        TipoIdentificacion: src.TipoIdentificacion,
        NumeroIdentificacion: src.NumeroIdentificacion,
        Correo: src.Correo,
        Telefono: src.Telefono);

    public static ConductorUpsertRequest ToUpsert(ConductorReservaRequest src) => new(
        Nombres: src.Nombres,
        Apellidos: src.Apellidos,
        TipoIdentificacion: src.TipoIdentificacion,
        NumeroIdentificacion: src.NumeroIdentificacion,
        FechaVencimientoLicencia: src.FechaVencimientoLicencia,
        EdadConductor: src.EdadConductor,
        Correo: src.Correo,
        Telefono: src.Telefono,
        EsPrincipal: src.EsPrincipal);
}
