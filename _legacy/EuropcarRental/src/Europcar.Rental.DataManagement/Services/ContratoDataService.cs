using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class ContratoDataService : IContratoDataService
{
    private readonly RentalDbContext _context;
    public ContratoDataService(RentalDbContext context) => _context = context;

    public async Task<ContratoModel?> GetByIdAsync(int id)
    {
        var c = await _context.Contratos
            .Include(c => c.Cliente)
            .Include(c => c.Vehiculo)
            .Include(c => c.Reserva)
            .FirstOrDefaultAsync(c => c.IdContrato == id);
        return c == null ? null : MapToModel(c);
    }

    public async Task<ContratoModel?> GetByReservaIdAsync(int idReserva)
    {
        var c = await _context.Contratos
            .Include(c => c.Cliente)
            .Include(c => c.Vehiculo)
            .Include(c => c.Reserva)
            .FirstOrDefaultAsync(c => c.IdReserva == idReserva);
        return c == null ? null : MapToModel(c);
    }

    public async Task<IEnumerable<ContratoModel>> GetAllAsync()
    {
        return await _context.Contratos
            .Include(c => c.Cliente)
            .Include(c => c.Vehiculo)
            .Include(c => c.Reserva)
            .OrderByDescending(c => c.FechaRegistroUtc)
            .Select(c => MapToModel(c))
            .ToListAsync();
    }

    public async Task<IEnumerable<ContratoModel>> GetByClienteIdAsync(int idCliente)
    {
        return await _context.Contratos
            .Include(c => c.Cliente)
            .Include(c => c.Vehiculo)
            .Include(c => c.Reserva)
            .Where(c => c.IdCliente == idCliente)
            .OrderByDescending(c => c.FechaRegistroUtc)
            .Select(c => MapToModel(c))
            .ToListAsync();
    }

    public async Task<ContratoModel> CreateAsync(ContratoModel model, string usuario)
    {
        var entity = new ContratoEntity
        {
            ContratoGuid = Guid.NewGuid(),
            NumeroContrato = model.NumeroContrato,
            IdReserva = model.IdReserva,
            IdCliente = model.IdCliente,
            IdVehiculo = model.IdVehiculo,
            FechaHoraSalida = model.FechaHoraSalida,
            FechaHoraPrevistaDevolucion = model.FechaHoraPrevistaDevolucion,
            KilometrajeSalida = model.KilometrajeSalida,
            NivelCombustibleSalida = model.NivelCombustibleSalida,
            EstadoContrato = "ABIERTO",
            PdfUrl = model.PdfUrl,
            ObservacionesContrato = model.ObservacionesContrato,
            CreadoPorUsuario = usuario,
            OrigenRegistro = "API",
            FechaRegistroUtc = DateTimeOffset.UtcNow
        };
        await _context.Contratos.AddAsync(entity);
        model.IdContrato = entity.IdContrato;
        model.ContratoGuid = entity.ContratoGuid;
        return model;
    }

    public async Task UpdateEstadoAsync(int id, string estado, string usuario)
    {
        var entity = await _context.Contratos.FindAsync(id);
        if (entity != null)
        {
            entity.EstadoContrato = estado;
            entity.ModificadoPorUsuario = usuario;
            entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        }
    }

    private static ContratoModel MapToModel(ContratoEntity c) => new()
    {
        IdContrato = c.IdContrato,
        ContratoGuid = c.ContratoGuid,
        NumeroContrato = c.NumeroContrato,
        IdReserva = c.IdReserva,
        IdCliente = c.IdCliente,
        IdVehiculo = c.IdVehiculo,
        FechaHoraSalida = c.FechaHoraSalida,
        FechaHoraPrevistaDevolucion = c.FechaHoraPrevistaDevolucion,
        KilometrajeSalida = c.KilometrajeSalida,
        NivelCombustibleSalida = c.NivelCombustibleSalida,
        EstadoContrato = c.EstadoContrato,
        PdfUrl = c.PdfUrl,
        ObservacionesContrato = c.ObservacionesContrato,
        NombreCliente = c.Cliente != null ? $"{c.Cliente.CliNombre1} {c.Cliente.CliApellido1}" : null,
        PlacaVehiculo = c.Vehiculo?.PlacaVehiculo,
        CodigoReserva = c.Reserva?.CodigoReserva
    };
}
