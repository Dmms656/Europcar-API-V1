using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class CiudadDataService : ICiudadDataService
{
    private readonly RentalDbContext _context;
    public CiudadDataService(RentalDbContext context) => _context = context;

    public async Task<IEnumerable<CiudadModel>> GetAllAsync()
    {
        return await _context.Ciudades
            .Include(c => c.Pais)
            .Where(c => c.EstadoCiudad == "ACT")
            .OrderBy(c => c.NombreCiudad)
            .Select(c => new CiudadModel
            {
                IdCiudad = c.IdCiudad,
                CiudadGuid = c.CiudadGuid,
                IdPais = c.IdPais,
                NombreCiudad = c.NombreCiudad,
                NombrePais = c.Pais.NombrePais,
                EstadoCiudad = c.EstadoCiudad
            })
            .ToListAsync();
    }

    public async Task<CiudadModel?> GetByIdAsync(int id)
    {
        var c = await _context.Ciudades
            .Include(c => c.Pais)
            .FirstOrDefaultAsync(c => c.IdCiudad == id);
        if (c == null) return null;
        return new CiudadModel
        {
            IdCiudad = c.IdCiudad,
            CiudadGuid = c.CiudadGuid,
            IdPais = c.IdPais,
            NombreCiudad = c.NombreCiudad,
            NombrePais = c.Pais?.NombrePais,
            EstadoCiudad = c.EstadoCiudad
        };
    }
}
