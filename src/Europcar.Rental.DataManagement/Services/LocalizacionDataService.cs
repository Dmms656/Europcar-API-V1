using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class LocalizacionDataService : ILocalizacionDataService
{
    private readonly RentalDbContext _context;
    public LocalizacionDataService(RentalDbContext context) => _context = context;

    public async Task<IEnumerable<LocalizacionModel>> GetAllAsync(bool soloActivas = true)
    {
        var query = _context.Localizaciones
            .Include(l => l.Ciudad)
            .AsQueryable();

        if (soloActivas)
            query = query.Where(l => l.EstadoLocalizacion == "ACT");

        return await query
            .OrderBy(l => l.NombreLocalizacion)
            .Select(l => MapToModel(l))
            .ToListAsync();
    }

    public async Task<LocalizacionModel?> GetByIdAsync(int id)
    {
        var l = await _context.Localizaciones
            .Include(l => l.Ciudad)
            .FirstOrDefaultAsync(l => l.IdLocalizacion == id);
        return l == null ? null : MapToModel(l);
    }

    public async Task<LocalizacionModel?> GetByCodigoAsync(string codigo)
    {
        var l = await _context.Localizaciones
            .Include(l => l.Ciudad)
            .FirstOrDefaultAsync(l => l.CodigoLocalizacion == codigo);
        return l == null ? null : MapToModel(l);
    }

    public async Task<LocalizacionModel> CreateAsync(LocalizacionModel model, string usuario)
    {
        var entity = new LocalizacionEntity
        {
            LocalizacionGuid = Guid.NewGuid(),
            CodigoLocalizacion = model.CodigoLocalizacion.Trim().ToUpper(),
            NombreLocalizacion = model.NombreLocalizacion.Trim(),
            IdCiudad = model.IdCiudad,
            DireccionLocalizacion = model.DireccionLocalizacion.Trim(),
            TelefonoContacto = model.TelefonoContacto.Trim(),
            CorreoContacto = model.CorreoContacto.Trim().ToLower(),
            HorarioAtencion = model.HorarioAtencion.Trim(),
            ZonaHoraria = string.IsNullOrWhiteSpace(model.ZonaHoraria) ? "America/Guayaquil" : model.ZonaHoraria,
            Latitud = model.Latitud,
            Longitud = model.Longitud,
            EstadoLocalizacion = "ACT",
            CreadoPorUsuario = usuario,
            OrigenRegistro = "API",
            FechaRegistroUtc = DateTimeOffset.UtcNow
        };

        await _context.Localizaciones.AddAsync(entity);
        await _context.SaveChangesAsync();

        model.IdLocalizacion = entity.IdLocalizacion;
        model.LocalizacionGuid = entity.LocalizacionGuid;
        model.RowVersion = entity.RowVersion;
        return model;
    }

    public async Task UpdateAsync(LocalizacionModel model, string usuario)
    {
        var entity = await _context.Localizaciones.FindAsync(model.IdLocalizacion)
            ?? throw new InvalidOperationException($"Localización {model.IdLocalizacion} no encontrada");

        entity.NombreLocalizacion = model.NombreLocalizacion.Trim();
        entity.IdCiudad = model.IdCiudad;
        entity.DireccionLocalizacion = model.DireccionLocalizacion.Trim();
        entity.TelefonoContacto = model.TelefonoContacto.Trim();
        entity.CorreoContacto = model.CorreoContacto.Trim().ToLower();
        entity.HorarioAtencion = model.HorarioAtencion.Trim();
        entity.ZonaHoraria = string.IsNullOrWhiteSpace(model.ZonaHoraria) ? entity.ZonaHoraria : model.ZonaHoraria;
        entity.Latitud = model.Latitud;
        entity.Longitud = model.Longitud;
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateEstadoAsync(int id, string estado, string usuario, string? motivo = null)
    {
        var entity = await _context.Localizaciones.FindAsync(id)
            ?? throw new InvalidOperationException($"Localización {id} no encontrada");

        entity.EstadoLocalizacion = estado;
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;

        if (estado == "INA")
        {
            entity.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
            entity.MotivoInhabilitacion = motivo;
        }
        else
        {
            entity.FechaInhabilitacionUtc = null;
            entity.MotivoInhabilitacion = null;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id, string usuario, string? motivo = null)
    {
        var entity = await _context.Localizaciones.FindAsync(id)
            ?? throw new InvalidOperationException($"Localización {id} no encontrada");

        entity.EsEliminado = true;
        entity.EstadoLocalizacion = "INA";
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        entity.MotivoInhabilitacion = motivo ?? "Eliminada por administrador";

        await _context.SaveChangesAsync();
    }

    private static LocalizacionModel MapToModel(LocalizacionEntity l) => new()
    {
        IdLocalizacion = l.IdLocalizacion,
        LocalizacionGuid = l.LocalizacionGuid,
        CodigoLocalizacion = l.CodigoLocalizacion,
        NombreLocalizacion = l.NombreLocalizacion,
        IdCiudad = l.IdCiudad,
        DireccionLocalizacion = l.DireccionLocalizacion,
        TelefonoContacto = l.TelefonoContacto,
        CorreoContacto = l.CorreoContacto,
        HorarioAtencion = l.HorarioAtencion,
        ZonaHoraria = l.ZonaHoraria,
        Latitud = l.Latitud,
        Longitud = l.Longitud,
        NombreCiudad = l.Ciudad?.NombreCiudad,
        EstadoLocalizacion = l.EstadoLocalizacion,
        RowVersion = l.RowVersion
    };
}
