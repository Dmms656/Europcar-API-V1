using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class VehiculoDataService : IVehiculoDataService
{
    private readonly RentalDbContext _context;

    public VehiculoDataService(RentalDbContext context) => _context = context;

    public async Task<IEnumerable<VehiculoModel>> GetDisponiblesAsync(int? localizacionId, int? categoriaId)
    {
        var query = _context.Vehiculos
            .Include(v => v.Marca)
            .Include(v => v.Categoria)
            .Include(v => v.Localizacion)
            .Where(v => v.EstadoOperativo == "DISPONIBLE" && v.EstadoVehiculo == "ACT");

        if (localizacionId.HasValue)
            query = query.Where(v => v.LocalizacionActual == localizacionId.Value);

        if (categoriaId.HasValue)
            query = query.Where(v => v.IdCategoria == categoriaId.Value);

        return await query.Select(v => MapToModel(v)).ToListAsync();
    }

    public async Task<IEnumerable<VehiculoModel>> GetAllAsync()
    {
        return await _context.Vehiculos
            .Include(v => v.Marca)
            .Include(v => v.Categoria)
            .Include(v => v.Localizacion)
            .Where(v => v.EstadoVehiculo == "ACT")
            .Select(v => MapToModel(v))
            .ToListAsync();
    }

    public async Task<VehiculoModel?> GetByIdAsync(int id)
    {
        var v = await _context.Vehiculos
            .Include(v => v.Marca)
            .Include(v => v.Categoria)
            .Include(v => v.Localizacion)
            .FirstOrDefaultAsync(v => v.IdVehiculo == id);

        if (v == null) return null;
        return MapToModel(v);
    }

    public async Task UpdateEstadoOperativoAsync(int id, string estado, string usuario)
    {
        var entity = await _context.Vehiculos.FindAsync(id);
        if (entity != null)
        {
            entity.EstadoOperativo = estado;
            entity.ModificadoPorUsuario = usuario;
            entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        }
    }

    private static VehiculoModel MapToModel(DataAccess.Entities.Rental.VehiculoEntity v) => new()
    {
        IdVehiculo = v.IdVehiculo,
        VehiculoGuid = v.VehiculoGuid,
        CodigoInterno = v.CodigoInternoVehiculo,
        Placa = v.PlacaVehiculo,
        Marca = v.Marca.NombreMarca,
        Categoria = v.Categoria.NombreCategoria,
        Modelo = v.ModeloVehiculo,
        AnioFabricacion = v.AnioFabricacion,
        Color = v.ColorVehiculo,
        TipoCombustible = v.TipoCombustible,
        TipoTransmision = v.TipoTransmision,
        CapacidadPasajeros = v.CapacidadPasajeros,
        CapacidadMaletas = v.CapacidadMaletas,
        NumeroPuertas = v.NumeroPuertas,
        PrecioBaseDia = v.PrecioBaseDia,
        KilometrajeActual = v.KilometrajeActual,
        AireAcondicionado = v.AireAcondicionado,
        EstadoOperativo = v.EstadoOperativo,
        ImagenUrl = v.ImagenReferencialUrl,
        IdLocalizacion = v.LocalizacionActual,
        NombreLocalizacion = v.Localizacion.NombreLocalizacion
    };
}
