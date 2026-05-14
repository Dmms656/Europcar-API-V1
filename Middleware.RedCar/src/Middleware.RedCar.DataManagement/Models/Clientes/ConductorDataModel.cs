namespace Middleware.RedCar.DataManagement.Models.Clientes;

public sealed record ConductorDataModel(
    int IdConductor,
    Guid ConductorGuid,
    int IdCliente,
    string Nombres,
    string Apellidos,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    DateOnly FechaVencimientoLicencia,
    int EdadConductor,
    string Correo,
    string Telefono,
    bool EsPrincipal);
