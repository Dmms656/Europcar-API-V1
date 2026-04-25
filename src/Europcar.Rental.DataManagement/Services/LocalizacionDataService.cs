using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class LocalizacionDataService : ILocalizacionDataService
{
    private readonly RentalDbContext _context;
    public LocalizacionDataService(RentalDbContext context) => _context = context;

    public async Task<IEnumerable<LocalizacionModel>> GetAllAsync()
    {
        return await _context.Localizaciones
            .Include(l => l.Ciudad)
            .Where(l => l.EstadoLocalizacion == "ACT")
            .Select(l => new LocalizacionModel
            {
                IdLocalizacion = l.IdLocalizacion,
                LocalizacionGuid = l.LocalizacionGuid,
                CodigoLocalizacion = l.CodigoLocalizacion,
                NombreLocalizacion = l.NombreLocalizacion,
                DireccionLocalizacion = l.DireccionLocalizacion,
                TelefonoContacto = l.TelefonoContacto,
                CorreoContacto = l.CorreoContacto,
                HorarioAtencion = l.HorarioAtencion,
                NombreCiudad = l.Ciudad.NombreCiudad,
                EstadoLocalizacion = l.EstadoLocalizacion
            }).ToListAsync();
    }

    public async Task<LocalizacionModel?> GetByIdAsync(int id)
    {
        var l = await _context.Localizaciones
            .Include(l => l.Ciudad)
            .FirstOrDefaultAsync(l => l.IdLocalizacion == id);
        if (l == null) return null;
        return new LocalizacionModel
        {
            IdLocalizacion = l.IdLocalizacion,
            LocalizacionGuid = l.LocalizacionGuid,
            CodigoLocalizacion = l.CodigoLocalizacion,
            NombreLocalizacion = l.NombreLocalizacion,
            DireccionLocalizacion = l.DireccionLocalizacion,
            TelefonoContacto = l.TelefonoContacto,
            CorreoContacto = l.CorreoContacto,
            HorarioAtencion = l.HorarioAtencion,
            NombreCiudad = l.Ciudad.NombreCiudad,
            EstadoLocalizacion = l.EstadoLocalizacion
        };
    }
}
