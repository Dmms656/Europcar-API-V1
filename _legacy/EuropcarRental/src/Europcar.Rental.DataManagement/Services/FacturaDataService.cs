using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Europcar.Rental.DataManagement.Services;

public class FacturaDataService : IFacturaDataService
{
    private readonly RentalDbContext _context;
    public FacturaDataService(RentalDbContext context) => _context = context;

    public async Task<FacturaModel> AddAsync(FacturaModel model, string usuario)
    {
        var entity = BuildEntity(model, usuario);
        await _context.Facturas.AddAsync(entity);
        // No SaveChanges — caller handles it
        return model;
    }

    public async Task<FacturaModel> CreateAsync(FacturaModel model, string usuario)
    {
        var entity = BuildEntity(model, usuario);
        await _context.Facturas.AddAsync(entity);
        await _context.SaveChangesAsync();
        model.IdFactura = entity.IdFactura;
        model.FacturaGuid = entity.FacturaGuid;
        return model;
    }

    public async Task<IEnumerable<FacturaResumenModel>> GetByClienteIdAsync(int idCliente)
    {
        return await _context.Facturas
            .AsNoTracking()
            .Where(f => f.IdCliente == idCliente)
            .OrderByDescending(f => f.FechaEmision)
            .Select(f => new FacturaResumenModel
            {
                IdFactura = f.IdFactura,
                NumeroFactura = f.NumeroFactura,
                FechaEmision = f.FechaEmision,
                Subtotal = f.Subtotal,
                ValorIva = f.ValorIva,
                Total = f.Total,
                EstadoFactura = f.EstadoFactura,
                ServicioOrigen = f.ServicioOrigen,
                IdReserva = f.IdReserva,
                CodigoReserva = f.Reserva != null ? f.Reserva.CodigoReserva : null,
                IdContrato = f.IdContrato,
                NumeroContrato = f.Contrato != null ? f.Contrato.NumeroContrato : null
            })
            .ToListAsync();
    }

    private static FacturaEntity BuildEntity(FacturaModel model, string usuario) => new()
    {
        FacturaGuid = Guid.NewGuid(),
        NumeroFactura = model.NumeroFactura,
        IdCliente = model.IdCliente,
        IdReserva = model.IdReserva,
        IdContrato = model.IdContrato,
        FechaEmision = DateTimeOffset.UtcNow,
        Subtotal = model.Subtotal,
        ValorIva = model.ValorIva,
        Total = model.Total,
        EstadoFactura = model.EstadoFactura,
        ServicioOrigen = model.ServicioOrigen,
        OrigenCanalFactura = model.OrigenCanalFactura,
        ObservacionesFactura = model.ObservacionesFactura,
        CreadoPorUsuario = usuario,
        FechaRegistroUtc = DateTimeOffset.UtcNow
    };
}
