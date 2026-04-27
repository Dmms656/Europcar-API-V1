using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class PagoDataService : IPagoDataService
{
    private readonly RentalDbContext _context;
    public PagoDataService(RentalDbContext context) => _context = context;

    public async Task<PagoModel?> GetByIdAsync(int id)
    {
        var p = await _context.Pagos
            .Include(p => p.Cliente)
            .Include(p => p.Reserva)
            .FirstOrDefaultAsync(p => p.IdPago == id);
        return p == null ? null : MapToModel(p);
    }

    public async Task<IEnumerable<PagoModel>> GetByReservaIdAsync(int idReserva)
    {
        return await _context.Pagos
            .Include(p => p.Cliente)
            .Include(p => p.Reserva)
            .Where(p => p.IdReserva == idReserva)
            .OrderByDescending(p => p.FechaPagoUtc)
            .Select(p => MapToModel(p))
            .ToListAsync();
    }

    public async Task<PagoModel> CreateAsync(PagoModel model, string usuario)
    {
        var entity = new PagoEntity
        {
            PagoGuid = Guid.NewGuid(),
            CodigoPago = model.CodigoPago,
            IdReserva = model.IdReserva,
            IdContrato = model.IdContrato,
            IdCliente = model.IdCliente,
            TipoPago = model.TipoPago,
            MetodoPago = model.MetodoPago,
            EstadoPago = model.EstadoPago,
            ReferenciaExterna = model.ReferenciaExterna,
            Monto = model.Monto,
            Moneda = model.Moneda,
            FechaPagoUtc = DateTimeOffset.UtcNow,
            ObservacionesPago = model.ObservacionesPago,
            CreadoPorUsuario = usuario,
            OrigenRegistro = "API"
        };
        await _context.Pagos.AddAsync(entity);
        await _context.SaveChangesAsync();
        model.IdPago = entity.IdPago;
        model.PagoGuid = entity.PagoGuid;
        return model;
    }

    public async Task UpdateEstadoAsync(int id, string estado, string usuario)
    {
        var entity = await _context.Pagos.FindAsync(id);
        if (entity != null)
        {
            entity.EstadoPago = estado;
            entity.ModificadoPorUsuario = usuario;
            entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        }
    }

    private static PagoModel MapToModel(PagoEntity p) => new()
    {
        IdPago = p.IdPago,
        PagoGuid = p.PagoGuid,
        CodigoPago = p.CodigoPago,
        IdReserva = p.IdReserva,
        IdContrato = p.IdContrato,
        IdCliente = p.IdCliente,
        TipoPago = p.TipoPago,
        MetodoPago = p.MetodoPago,
        EstadoPago = p.EstadoPago,
        ReferenciaExterna = p.ReferenciaExterna,
        Monto = p.Monto,
        Moneda = p.Moneda,
        FechaPagoUtc = p.FechaPagoUtc,
        ObservacionesPago = p.ObservacionesPago,
        NombreCliente = p.Cliente != null ? $"{p.Cliente.CliNombre1} {p.Cliente.CliApellido1}" : null,
        CodigoReserva = p.Reserva?.CodigoReserva
    };
}
