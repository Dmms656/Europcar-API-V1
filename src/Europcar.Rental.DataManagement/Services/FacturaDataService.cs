using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class FacturaDataService : IFacturaDataService
{
    private readonly RentalDbContext _context;
    public FacturaDataService(RentalDbContext context) => _context = context;

    public async Task<FacturaModel> CreateAsync(FacturaModel model, string usuario)
    {
        var entity = new FacturaEntity
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

        await _context.Facturas.AddAsync(entity);
        await _context.SaveChangesAsync();

        model.IdFactura = entity.IdFactura;
        model.FacturaGuid = entity.FacturaGuid;
        return model;
    }
}
