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
            .OrderBy(e => e.NombreExtra)
            .Select(e => new CatalogoModel
            {
                Id = e.IdExtra,
                Guid = e.ExtraGuid,
                Codigo = e.CodigoExtra,
                Nombre = e.NombreExtra,
                Descripcion = e.DescripcionExtra,
                Estado = e.EstadoExtra,
                Tipo = e.TipoExtra,
                RequiereStock = e.RequiereStock,
                ValorFijo = e.ValorFijo
            }).ToListAsync();
    }

    public async Task<CatalogoModel?> GetExtraByIdAsync(int id)
    {
        var e = await _context.Extras.FirstOrDefaultAsync(x => x.IdExtra == id);
        if (e == null) return null;

        return new CatalogoModel
        {
            Id = e.IdExtra,
            Guid = e.ExtraGuid,
            Codigo = e.CodigoExtra,
            Nombre = e.NombreExtra,
            Descripcion = e.DescripcionExtra,
            Estado = e.EstadoExtra,
            Tipo = e.TipoExtra,
            RequiereStock = e.RequiereStock,
            ValorFijo = e.ValorFijo
        };
    }

    public async Task<CatalogoModel?> GetExtraByCodigoAsync(string codigo)
    {
        var e = await _context.Extras.FirstOrDefaultAsync(x => x.CodigoExtra == codigo);
        if (e == null) return null;

        return new CatalogoModel
        {
            Id = e.IdExtra,
            Guid = e.ExtraGuid,
            Codigo = e.CodigoExtra,
            Nombre = e.NombreExtra,
            Descripcion = e.DescripcionExtra,
            Estado = e.EstadoExtra,
            Tipo = e.TipoExtra,
            RequiereStock = e.RequiereStock,
            ValorFijo = e.ValorFijo
        };
    }

    public async Task<CatalogoModel> CreateExtraAsync(CatalogoModel model, string usuario)
    {
        var entity = new DataAccess.Entities.Rental.ExtraEntity
        {
            ExtraGuid = Guid.NewGuid(),
            CodigoExtra = model.Codigo.Trim().ToUpper(),
            NombreExtra = model.Nombre.Trim(),
            DescripcionExtra = model.Descripcion?.Trim() ?? string.Empty,
            TipoExtra = model.Tipo?.Trim().ToUpper() ?? "SERVICIO",
            RequiereStock = model.RequiereStock,
            ValorFijo = model.ValorFijo ?? 0m,
            EstadoExtra = "ACT",
            CreadoPorUsuario = usuario,
            FechaRegistroUtc = DateTimeOffset.UtcNow,
            OrigenRegistro = "API"
        };

        await _context.Extras.AddAsync(entity);
        await _context.SaveChangesAsync();

        model.Id = entity.IdExtra;
        model.Guid = entity.ExtraGuid;
        model.Estado = entity.EstadoExtra;
        return model;
    }

    public async Task UpdateExtraAsync(CatalogoModel model, string usuario)
    {
        var entity = await _context.Extras.FindAsync(model.Id)
            ?? throw new InvalidOperationException($"Extra {model.Id} no encontrado");

        entity.NombreExtra = model.Nombre.Trim();
        entity.DescripcionExtra = model.Descripcion?.Trim() ?? string.Empty;
        entity.TipoExtra = model.Tipo?.Trim().ToUpper() ?? entity.TipoExtra;
        entity.RequiereStock = model.RequiereStock;
        entity.ValorFijo = model.ValorFijo ?? entity.ValorFijo;
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateExtraEstadoAsync(int id, string estado, string usuario, string? motivo = null)
    {
        var entity = await _context.Extras.FindAsync(id)
            ?? throw new InvalidOperationException($"Extra {id} no encontrado");

        entity.EstadoExtra = estado;
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

    public async Task SoftDeleteExtraAsync(int id, string usuario, string? motivo = null)
    {
        var entity = await _context.Extras.FindAsync(id)
            ?? throw new InvalidOperationException($"Extra {id} no encontrado");

        entity.EsEliminado = true;
        entity.EstadoExtra = "INA";
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        entity.MotivoInhabilitacion = motivo ?? "Eliminado por administrador";

        await _context.SaveChangesAsync();
    }
}
