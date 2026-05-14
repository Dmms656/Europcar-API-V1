using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataAccess.Entities.Security;
using Europcar.Rental.DataManagement.Models;
using System;

namespace Europcar.Rental.DataManagement.Mappers;

/// <summary>
/// Extensiones de mapeo Entity ↔ Model para la capa DataManagement.
/// Desacopla la representación de EF Core de las capas superiores.
/// </summary>
public static class ClienteMapper
{
    public static ClienteModel ToModel(this ClienteEntity entity) => new()
    {
        IdCliente = entity.IdCliente,
        ClienteGuid = entity.ClienteGuid,
        CodigoCliente = entity.CodigoCliente,
        TipoIdentificacion = entity.TipoIdentificacion,
        NumeroIdentificacion = entity.NumeroIdentificacion,
        Nombre1 = entity.CliNombre1,
        Nombre2 = entity.CliNombre2,
        Apellido1 = entity.CliApellido1,
        Apellido2 = entity.CliApellido2,
        FechaNacimiento = entity.FechaNacimiento,
        Telefono = entity.CliTelefono,
        Correo = entity.CliCorreo,
        DireccionPrincipal = entity.DireccionPrincipal,
        EstadoCliente = entity.EstadoCliente,
        RowVersion = entity.RowVersion
    };

    public static ClienteEntity ToEntity(this ClienteModel model) => new()
    {
        ClienteGuid = model.ClienteGuid == Guid.Empty ? Guid.NewGuid() : model.ClienteGuid,
        CodigoCliente = model.CodigoCliente,
        TipoIdentificacion = model.TipoIdentificacion,
        NumeroIdentificacion = model.NumeroIdentificacion,
        CliNombre1 = model.Nombre1,
        CliNombre2 = model.Nombre2,
        CliApellido1 = model.Apellido1,
        CliApellido2 = model.Apellido2,
        FechaNacimiento = model.FechaNacimiento,
        CliTelefono = model.Telefono,
        CliCorreo = model.Correo,
        DireccionPrincipal = model.DireccionPrincipal,
        EstadoCliente = model.EstadoCliente,
        CreadoPorUsuario = "API",
        OrigenRegistro = "API",
        FechaRegistroUtc = DateTimeOffset.UtcNow
    };

    public static void ApplyUpdate(this ClienteEntity entity, ClienteModel model)
    {
        entity.TipoIdentificacion = model.TipoIdentificacion;
        entity.NumeroIdentificacion = model.NumeroIdentificacion;
        entity.CliNombre1 = model.Nombre1;
        entity.CliNombre2 = model.Nombre2;
        entity.CliApellido1 = model.Apellido1;
        entity.CliApellido2 = model.Apellido2;
        entity.FechaNacimiento = model.FechaNacimiento;
        entity.CliTelefono = model.Telefono;
        entity.CliCorreo = model.Correo;
        entity.DireccionPrincipal = model.DireccionPrincipal;
        entity.ModificadoPorUsuario = "API";
    }
}
