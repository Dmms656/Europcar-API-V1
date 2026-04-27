using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class MantenimientoDataService : IMantenimientoDataService
{
    private readonly RentalDbContext _context;
    public MantenimientoDataService(RentalDbContext context) => _context = context;

    public async Task<IEnumerable<MantenimientoModel>> GetAllAsync()
    {
        return await _context.Mantenimientos
            .Include(m => m.Vehiculo)
            .OrderByDescending(m => m.FechaInicioUtc)
            .Select(m => MapToModel(m))
            .ToListAsync();
    }

    public async Task<MantenimientoModel?> GetByIdAsync(int id)
    {
        var m = await _context.Mantenimientos
            .Include(m => m.Vehiculo)
            .FirstOrDefaultAsync(m => m.IdMantenimiento == id);
        return m == null ? null : MapToModel(m);
    }

    public async Task<IEnumerable<MantenimientoModel>> GetByVehiculoIdAsync(int idVehiculo)
    {
        return await _context.Mantenimientos
            .Include(m => m.Vehiculo)
            .Where(m => m.IdVehiculo == idVehiculo)
            .OrderByDescending(m => m.FechaInicioUtc)
            .Select(m => MapToModel(m))
            .ToListAsync();
    }

    public async Task<MantenimientoModel> CreateAsync(MantenimientoModel model, string usuario)
    {
        var entity = new MantenimientoEntity
        {
            MantenimientoGuid = Guid.NewGuid(),
            CodigoMantenimiento = model.CodigoMantenimiento,
            IdVehiculo = model.IdVehiculo,
            TipoMantenimiento = model.TipoMantenimiento,
            FechaInicioUtc = model.FechaInicioUtc,
            KilometrajeMantenimiento = model.KilometrajeMantenimiento,
            CostoMantenimiento = model.CostoMantenimiento,
            ProveedorTaller = model.ProveedorTaller,
            EstadoMantenimiento = "ABIERTO",
            Observaciones = model.Observaciones,
            CreadoPorUsuario = usuario,
            FechaRegistroUtc = DateTimeOffset.UtcNow
        };
        await _context.Mantenimientos.AddAsync(entity);
        model.IdMantenimiento = entity.IdMantenimiento;
        model.MantenimientoGuid = entity.MantenimientoGuid;
        return model;
    }

    public async Task CerrarAsync(int id, string usuario)
    {
        var entity = await _context.Mantenimientos.FindAsync(id);
        if (entity != null)
        {
            entity.EstadoMantenimiento = "CERRADO";
            entity.FechaFinUtc = DateTimeOffset.UtcNow;
        }
    }

    private static MantenimientoModel MapToModel(MantenimientoEntity m) => new()
    {
        IdMantenimiento = m.IdMantenimiento,
        MantenimientoGuid = m.MantenimientoGuid,
        CodigoMantenimiento = m.CodigoMantenimiento,
        IdVehiculo = m.IdVehiculo,
        TipoMantenimiento = m.TipoMantenimiento,
        FechaInicioUtc = m.FechaInicioUtc,
        FechaFinUtc = m.FechaFinUtc,
        KilometrajeMantenimiento = m.KilometrajeMantenimiento,
        CostoMantenimiento = m.CostoMantenimiento,
        ProveedorTaller = m.ProveedorTaller,
        EstadoMantenimiento = m.EstadoMantenimiento,
        Observaciones = m.Observaciones,
        PlacaVehiculo = m.Vehiculo?.PlacaVehiculo
    };
}
