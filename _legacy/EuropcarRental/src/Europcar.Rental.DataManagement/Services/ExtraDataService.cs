using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class ExtraDataService : IExtraDataService
{
    private readonly RentalDbContext _context;
    public ExtraDataService(RentalDbContext context) => _context = context;

    public async Task<ExtraDetailModel?> GetByIdAsync(int id)
    {
        var e = await _context.Extras
            .FirstOrDefaultAsync(e => e.IdExtra == id && e.EstadoExtra == "ACT");
        if (e == null) return null;

        return new ExtraDetailModel
        {
            IdExtra = e.IdExtra,
            CodigoExtra = e.CodigoExtra,
            NombreExtra = e.NombreExtra,
            TipoExtra = e.TipoExtra,
            RequiereStock = e.RequiereStock,
            ValorFijo = e.ValorFijo
        };
    }

    public async Task<int> GetStockDisponibleAsync(int idLocalizacion, int idExtra)
    {
        var stock = await _context.LocalizacionExtraStock
            .FirstOrDefaultAsync(s =>
                s.IdLocalizacion == idLocalizacion
                && s.IdExtra == idExtra
                && s.EstadoStock == "ACT");

        if (stock == null) return 0;
        return stock.StockDisponible - stock.StockReservado;
    }

    public async Task ReservarStockAsync(int idLocalizacion, int idExtra, int cantidad)
    {
        var stock = await _context.LocalizacionExtraStock
            .FirstOrDefaultAsync(s =>
                s.IdLocalizacion == idLocalizacion
                && s.IdExtra == idExtra
                && s.EstadoStock == "ACT");

        if (stock == null)
            throw new InvalidOperationException(
                $"No existe registro de stock para extra {idExtra} en localización {idLocalizacion}");

        var disponibleReal = stock.StockDisponible - stock.StockReservado;
        if (disponibleReal < cantidad)
            throw new InvalidOperationException(
                $"Stock insuficiente para extra {idExtra}. Disponible: {disponibleReal}, solicitado: {cantidad}");

        stock.StockReservado += cantidad;
        stock.ModificadoPorUsuario = "API";
        stock.FechaModificacionUtc = DateTimeOffset.UtcNow;
    }

    public async Task LiberarStockAsync(int idLocalizacion, int idExtra, int cantidad)
    {
        var stock = await _context.LocalizacionExtraStock
            .FirstOrDefaultAsync(s =>
                s.IdLocalizacion == idLocalizacion
                && s.IdExtra == idExtra
                && s.EstadoStock == "ACT");

        if (stock != null)
        {
            stock.StockReservado = Math.Max(0, stock.StockReservado - cantidad);
            stock.ModificadoPorUsuario = "API";
            stock.FechaModificacionUtc = DateTimeOffset.UtcNow;
        }
    }
}
