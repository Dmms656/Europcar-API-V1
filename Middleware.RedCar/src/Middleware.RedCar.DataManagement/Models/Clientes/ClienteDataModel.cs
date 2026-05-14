namespace Middleware.RedCar.DataManagement.Models.Clientes;

public sealed record ClienteDataModel(
    int IdCliente,
    Guid ClienteGuid,
    string Nombres,
    string Apellidos,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string Correo,
    string Telefono);
