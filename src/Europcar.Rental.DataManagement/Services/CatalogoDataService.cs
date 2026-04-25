using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class CatalogoDataService : ICatalogoDataService
{
    private readonly RentalDbContext _context;
    public CatalogoDataService(RentalDbContext context) => _context = context;

    public async Task<IEnumerable<CatalogoModel>> GetCategoriasAsync()
    {
        return await _context.CategoriaVehiculos
            .Where(c => c.EstadoCategoria == "ACT")
            .Select(c => new CatalogoModel
            {
                Id = c.IdCategoria,
                Guid = c.CategoriaGuid,
                Codigo = c.CodigoCategoria,
                Nombre = c.NombreCategoria,
                Descripcion = c.DescripcionCategoria,
                Estado = c.EstadoCategoria
            }).ToListAsync();
    }

    public async Task<IEnumerable<CatalogoModel>> GetMarcasAsync()
    {
        return await _context.MarcaVehiculos
            .Where(m => m.EstadoMarca == "ACT")
            .Select(m => new CatalogoModel
            {
                Id = m.IdMarca,
                Guid = m.MarcaGuid,
                Codigo = m.CodigoMarca,
                Nombre = m.NombreMarca,
                Descripcion = m.DescripcionMarca,
                Estado = m.EstadoMarca
            }).ToListAsync();
    }

    public async Task<IEnumerable<CatalogoModel>> GetExtrasAsync()
    {
        return await _context.Extras
            .Where(e => e.EstadoExtra == "ACT")
            .Select(e => new CatalogoModel
            {
                Id = e.IdExtra,
                Guid = e.ExtraGuid,
                Codigo = e.CodigoExtra,
                Nombre = e.NombreExtra,
                Descripcion = e.DescripcionExtra,
                Estado = e.EstadoExtra
            }).ToListAsync();
    }
}
