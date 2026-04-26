using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;
using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataManagement.Services;

public class ConductorDataService : IConductorDataService
{
    private readonly RentalDbContext _context;

    public ConductorDataService(RentalDbContext context) => _context = context;

    public async Task<IEnumerable<ConductorModel>> GetByClienteIdAsync(int idCliente)
    {
        return await _context.Conductores
            .Where(c => c.IdCliente == idCliente && c.EstadoConductor == "ACT")
            .Select(c => MapToModel(c))
            .ToListAsync();
    }

    public async Task<ConductorModel?> GetByIdAsync(int id)
    {
        var entity = await _context.Conductores.FindAsync(id);
        return entity == null ? null : MapToModel(entity);
    }

    public async Task<ConductorModel> CreateAsync(ConductorModel model)
    {
        var entity = new ConductorEntity
        {
            ConductorGuid = Guid.NewGuid(),
            CodigoConductor = $"CON-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            IdCliente = model.IdCliente,
            TipoIdentificacion = model.TipoIdentificacion,
            NumeroIdentificacion = model.NumeroIdentificacion,
            ConNombre1 = model.Nombre1,
            ConNombre2 = model.Nombre2,
            ConApellido1 = model.Apellido1,
            ConApellido2 = model.Apellido2,
            NumeroLicencia = model.NumeroLicencia,
            FechaVencimientoLicencia = model.FechaVencimientoLicencia,
            EdadConductor = model.EdadConductor,
            ConTelefono = model.Telefono,
            ConCorreo = model.Correo,
            EstadoConductor = "ACT"
        };

        _context.Conductores.Add(entity);
        await _context.SaveChangesAsync();

        return MapToModel(entity);
    }

    public async Task UpdateAsync(ConductorModel model)
    {
        var entity = await _context.Conductores.FindAsync(model.IdConductor)
            ?? throw new InvalidOperationException("Conductor no encontrado");

        entity.ConNombre1 = model.Nombre1;
        entity.ConNombre2 = model.Nombre2;
        entity.ConApellido1 = model.Apellido1;
        entity.ConApellido2 = model.Apellido2;
        entity.NumeroLicencia = model.NumeroLicencia;
        entity.FechaVencimientoLicencia = model.FechaVencimientoLicencia;
        entity.EdadConductor = model.EdadConductor;
        entity.ConTelefono = model.Telefono;
        entity.ConCorreo = model.Correo;

        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entity = await _context.Conductores.FindAsync(id);
        if (entity != null)
        {
            entity.EstadoConductor = "INA";
            await _context.SaveChangesAsync();
        }
    }

    private static ConductorModel MapToModel(ConductorEntity e) => new()
    {
        IdConductor = e.IdConductor,
        ConductorGuid = e.ConductorGuid,
        CodigoConductor = e.CodigoConductor,
        IdCliente = e.IdCliente,
        TipoIdentificacion = e.TipoIdentificacion,
        NumeroIdentificacion = e.NumeroIdentificacion,
        Nombre1 = e.ConNombre1,
        Nombre2 = e.ConNombre2,
        Apellido1 = e.ConApellido1,
        Apellido2 = e.ConApellido2,
        NumeroLicencia = e.NumeroLicencia,
        FechaVencimientoLicencia = e.FechaVencimientoLicencia,
        EdadConductor = e.EdadConductor,
        Telefono = e.ConTelefono,
        Correo = e.ConCorreo,
        EsConductorJoven = e.EsConductorJoven,
        EstadoConductor = e.EstadoConductor
    };
}
