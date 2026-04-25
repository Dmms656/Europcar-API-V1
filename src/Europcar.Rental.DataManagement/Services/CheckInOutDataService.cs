using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class CheckInOutDataService : ICheckInOutDataService
{
    private readonly RentalDbContext _context;
    public CheckInOutDataService(RentalDbContext context) => _context = context;

    public async Task<CheckInOutModel> CreateAsync(CheckInOutModel model, string usuario)
    {
        var entity = new CheckInOutEntity
        {
            CheckGuid = Guid.NewGuid(),
            IdContrato = model.IdContrato,
            TipoCheck = model.TipoCheck,
            FechaHoraCheck = model.FechaHoraCheck,
            Kilometraje = model.Kilometraje,
            NivelCombustible = model.NivelCombustible,
            Limpio = model.Limpio,
            Observaciones = model.Observaciones,
            CargoCombustible = model.CargoCombustible,
            CargoLimpieza = model.CargoLimpieza,
            CargoKmExtra = model.CargoKmExtra,
            CreadoPorUsuario = usuario,
            FechaRegistroUtc = DateTimeOffset.UtcNow
        };
        await _context.CheckInOuts.AddAsync(entity);
        model.IdCheck = entity.IdCheck;
        model.CheckGuid = entity.CheckGuid;
        return model;
    }

    public async Task<IEnumerable<CheckInOutModel>> GetByContratoIdAsync(int idContrato)
    {
        return await _context.CheckInOuts
            .Where(c => c.IdContrato == idContrato)
            .OrderBy(c => c.FechaHoraCheck)
            .Select(c => new CheckInOutModel
            {
                IdCheck = c.IdCheck,
                CheckGuid = c.CheckGuid,
                IdContrato = c.IdContrato,
                TipoCheck = c.TipoCheck,
                FechaHoraCheck = c.FechaHoraCheck,
                Kilometraje = c.Kilometraje,
                NivelCombustible = c.NivelCombustible,
                Limpio = c.Limpio,
                Observaciones = c.Observaciones,
                CargoCombustible = c.CargoCombustible,
                CargoLimpieza = c.CargoLimpieza,
                CargoKmExtra = c.CargoKmExtra
            }).ToListAsync();
    }
}
