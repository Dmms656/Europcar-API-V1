using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class ClienteDataService : IClienteDataService
{
    private readonly RentalDbContext _context;

    public ClienteDataService(RentalDbContext context) => _context = context;

    public async Task<IEnumerable<ClienteModel>> GetAllAsync()
    {
        return await _context.Clientes
            .Select(c => MapToModel(c))
            .ToListAsync();
    }

    public async Task<ClienteModel?> GetByIdAsync(int id)
    {
        var entity = await _context.Clientes.FindAsync(id);
        return entity == null ? null : MapToModel(entity);
    }

    public async Task<ClienteModel?> GetByIdentificacionAsync(string numeroIdentificacion)
    {
        var entity = await _context.Clientes
            .FirstOrDefaultAsync(c => c.NumeroIdentificacion == numeroIdentificacion);
        return entity == null ? null : MapToModel(entity);
    }

    public async Task<ClienteModel> CreateAsync(ClienteModel model)
    {
        var entity = new ClienteEntity
        {
            ClienteGuid = Guid.NewGuid(),
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
            EstadoCliente = "ACT",
            CreadoPorUsuario = "API",
            OrigenRegistro = "API",
            FechaRegistroUtc = DateTimeOffset.UtcNow
        };

        await _context.Clientes.AddAsync(entity);
        await _context.SaveChangesAsync();
        
        model.IdCliente = entity.IdCliente;
        model.ClienteGuid = entity.ClienteGuid;
        return model;
    }

    public async Task UpdateAsync(ClienteModel model)
    {
        var entity = await _context.Clientes.FindAsync(model.IdCliente)
            ?? throw new InvalidOperationException($"Cliente {model.IdCliente} no encontrado");

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
        entity.RowVersion = model.RowVersion;
    }

    public async Task SoftDeleteAsync(int id, string usuario)
    {
        var entity = await _context.Clientes.FindAsync(id)
            ?? throw new InvalidOperationException($"Cliente {id} no encontrado");

        entity.EsEliminado = true;
        entity.EstadoCliente = "INA";
        entity.ModificadoPorUsuario = usuario;
        entity.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
    }

    private static ClienteModel MapToModel(ClienteEntity e) => new()
    {
        IdCliente = e.IdCliente,
        ClienteGuid = e.ClienteGuid,
        CodigoCliente = e.CodigoCliente,
        TipoIdentificacion = e.TipoIdentificacion,
        NumeroIdentificacion = e.NumeroIdentificacion,
        Nombre1 = e.CliNombre1,
        Nombre2 = e.CliNombre2,
        Apellido1 = e.CliApellido1,
        Apellido2 = e.CliApellido2,
        FechaNacimiento = e.FechaNacimiento,
        Telefono = e.CliTelefono,
        Correo = e.CliCorreo,
        DireccionPrincipal = e.DireccionPrincipal,
        EstadoCliente = e.EstadoCliente,
        RowVersion = e.RowVersion
    };
}
