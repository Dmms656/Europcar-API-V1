using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Rental;
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

        return (await query.ToListAsync()).Select(v => MapToModel(v));
    }

    public async Task<IEnumerable<VehiculoModel>> GetAllAsync()
    {
        var entities = await _context.Vehiculos
            .Include(v => v.Marca)
            .Include(v => v.Categoria)
            .Include(v => v.Localizacion)
            .Where(v => v.EstadoVehiculo == "ACT")
            .ToListAsync();

        return entities.Select(v => MapToModel(v));
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

    public async Task<VehiculoModel?> GetByPlacaAsync(string placa)
    {
        var v = await _context.Vehiculos
            .Include(v => v.Marca)
            .Include(v => v.Categoria)
            .Include(v => v.Localizacion)
            .FirstOrDefaultAsync(v => v.PlacaVehiculo == placa);

        if (v == null) return null;
        return MapToModel(v);
    }

    public async Task<VehiculoModel> CreateAsync(VehiculoModel model)
    {
        var entity = new VehiculoEntity
        {
            VehiculoGuid = Guid.NewGuid(),
            CodigoInternoVehiculo = model.CodigoInterno,
            PlacaVehiculo = model.Placa,
            IdMarca = model.IdMarca,
            IdCategoria = model.IdCategoria,
            ModeloVehiculo = model.Modelo,
            AnioFabricacion = model.AnioFabricacion,
            ColorVehiculo = model.Color,
            TipoCombustible = model.TipoCombustible,
            TipoTransmision = model.TipoTransmision,
            CapacidadPasajeros = model.CapacidadPasajeros,
            CapacidadMaletas = model.CapacidadMaletas,
            NumeroPuertas = model.NumeroPuertas,
            LocalizacionActual = model.IdLocalizacion,
            PrecioBaseDia = model.PrecioBaseDia,
            KilometrajeActual = model.KilometrajeActual,
            AireAcondicionado = model.AireAcondicionado,
            EstadoOperativo = "DISPONIBLE",
            ObservacionesGenerales = model.ObservacionesGenerales,
            ImagenReferencialUrl = model.ImagenUrl,
            EstadoVehiculo = "ACT",
            CreadoPorUsuario = "API",
            OrigenRegistro = "API",
            FechaRegistroUtc = DateTimeOffset.UtcNow
        };

        await _context.Vehiculos.AddAsync(entity);
        // SaveChanges is called by UnitOfWork

        model.IdVehiculo = entity.IdVehiculo;
        model.VehiculoGuid = entity.VehiculoGuid;
        return model;
    }

    public async Task UpdateAsync(VehiculoModel model)
    {
        var entity = await _context.Vehiculos.FindAsync(model.IdVehiculo)
            ?? throw new InvalidOperationException($"Vehículo {model.IdVehiculo} no encontrado");

        entity.PlacaVehiculo = model.Placa;
        entity.IdMarca = model.IdMarca;
        entity.IdCategoria = model.IdCategoria;
        entity.ModeloVehiculo = model.Modelo;
        entity.AnioFabricacion = model.AnioFabricacion;
        entity.ColorVehiculo = model.Color;
        entity.TipoCombustible = model.TipoCombustible;
        entity.TipoTransmision = model.TipoTransmision;
        entity.CapacidadPasajeros = model.CapacidadPasajeros;
        entity.CapacidadMaletas = model.CapacidadMaletas;
        entity.NumeroPuertas = model.NumeroPuertas;
        entity.LocalizacionActual = model.IdLocalizacion;
        entity.PrecioBaseDia = model.PrecioBaseDia;
        entity.KilometrajeActual = model.KilometrajeActual;
        entity.AireAcondicionado = model.AireAcondicionado;
        entity.ObservacionesGenerales = model.ObservacionesGenerales;
        entity.ImagenReferencialUrl = model.ImagenUrl;
        entity.ModificadoPorUsuario = "API";
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion = model.RowVersion;
    }

    public async Task SoftDeleteAsync(int id, string usuario)
    {
        var entity = await _context.Vehiculos.FindAsync(id)
            ?? throw new InvalidOperationException($"Vehículo {id} no encontrado");

        entity.EsEliminado = true;
        entity.EstadoVehiculo = "INA";
        entity.EstadoOperativo = "FUERA_SERVICIO";
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
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

    private static VehiculoModel MapToModel(VehiculoEntity v) => new()
    {
        IdVehiculo = v.IdVehiculo,
        VehiculoGuid = v.VehiculoGuid,
        CodigoInterno = v.CodigoInternoVehiculo,
        Placa = v.PlacaVehiculo,
        IdMarca = v.IdMarca,
        Marca = v.Marca.NombreMarca,
        IdCategoria = v.IdCategoria,
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
        ObservacionesGenerales = v.ObservacionesGenerales,
        ImagenUrl = v.ImagenReferencialUrl,
        IdLocalizacion = v.LocalizacionActual,
        NombreLocalizacion = v.Localizacion.NombreLocalizacion,
        EstadoVehiculo = v.EstadoVehiculo,
        RowVersion = v.RowVersion
    };
}
