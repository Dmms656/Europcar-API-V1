using Middleware.RedCar.DataAccess.Clients.Interfaces;
using Middleware.RedCar.DataManagement.Models.Clientes;

namespace Middleware.RedCar.DataManagement.Mappers;

internal static class ClientesDataMapper
{
    public static ClienteDataModel ToData(ClienteUpsertRequest req, ClienteUpsertResult res) => new(
        IdCliente: res.IdCliente,
        ClienteGuid: res.ClienteGuid,
        Nombres: req.Nombres,
        Apellidos: req.Apellidos,
        TipoIdentificacion: req.TipoIdentificacion,
        NumeroIdentificacion: req.NumeroIdentificacion,
        Correo: req.Correo,
        Telefono: req.Telefono);

    public static ConductorDataModel ToData(int idCliente, ConductorUpsertRequest req, ConductorUpsertResult res) => new(
        IdConductor: res.IdConductor,
        ConductorGuid: res.ConductorGuid,
        IdCliente: idCliente,
        Nombres: req.Nombres,
        Apellidos: req.Apellidos,
        TipoIdentificacion: req.TipoIdentificacion,
        NumeroIdentificacion: req.NumeroIdentificacion,
        FechaVencimientoLicencia: req.FechaVencimientoLicencia,
        EdadConductor: req.EdadConductor,
        Correo: req.Correo,
        Telefono: req.Telefono,
        EsPrincipal: req.EsPrincipal);
}
