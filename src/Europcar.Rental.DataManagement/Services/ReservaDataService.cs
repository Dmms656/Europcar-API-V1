using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

public class ReservaDataService : IReservaDataService
{
    private readonly RentalDbContext _context;

    public ReservaDataService(RentalDbContext context) => _context = context;

    public async Task<ReservaModel?> GetByCodigoAsync(string codigo)
    {
        var r = await _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.Vehiculo)
            .Include(r => r.Extras).ThenInclude(e => e.Extra)
            .FirstOrDefaultAsync(r => r.CodigoReserva == codigo);

        if (r == null) return null;
        return MapToModel(r);
    }

    public async Task<ReservaModel?> GetByIdAsync(int id)
    {
        var r = await _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.Vehiculo).ThenInclude(v => v.Marca)
            .Include(r => r.Extras).ThenInclude(e => e.Extra)
            .FirstOrDefaultAsync(r => r.IdReserva == id);
        return r == null ? null : MapToModel(r);
    }

    public async Task<IEnumerable<ReservaModel>> GetByClienteIdAsync(int idCliente)
    {
        var reservas = await _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.Vehiculo).ThenInclude(v => v.Marca)
            .Include(r => r.Extras).ThenInclude(e => e.Extra)
            .Where(r => r.IdCliente == idCliente)
            .OrderByDescending(r => r.FechaHoraRecogida)
            .ToListAsync();

        return reservas.Select(MapToModel);
    }

    public async Task<ReservaModel> CreateAsync(ReservaModel model)
    {
        var entity = new ReservaEntity
        {
            ReservaGuid = Guid.NewGuid(),
            CodigoReserva = model.CodigoReserva,
            IdCliente = model.IdCliente,
            IdVehiculo = model.IdVehiculo,
            IdLocalizacionRecogida = model.IdLocalizacionRecogida,
            IdLocalizacionDevolucion = model.IdLocalizacionDevolucion,
            CanalReserva = model.CanalReserva,
            FechaHoraRecogida = model.FechaHoraRecogida,
            FechaHoraDevolucion = model.FechaHoraDevolucion,
            Subtotal = model.Subtotal,
            ValorImpuestos = model.ValorImpuestos,
            ValorExtras = model.ValorExtras,
            CargoOneWay = model.CargoOneWay,
            Total = model.Total,
            CodigoConfirmacion = model.CodigoConfirmacion,
            EstadoReserva = "PENDIENTE",
            CreadoPorUsuario = "API",
            OrigenRegistro = "API",
            FechaRegistroUtc = DateTimeOffset.UtcNow
        };

        await _context.Reservas.AddAsync(entity);

        // Se necesita el ID generado para insertar los extras
        await _context.SaveChangesAsync();

        // Insertar extras asociados
        if (model.Extras.Count > 0)
        {
            foreach (var extra in model.Extras)
            {
                var extraEntity = new ReservaExtraEntity
                {
                    ReservaExtraGuid = Guid.NewGuid(),
                    IdReserva = entity.IdReserva,
                    IdExtra = extra.IdExtra,
                    Cantidad = extra.Cantidad,
                    ValorUnitarioExtra = extra.ValorUnitario,
                    SubtotalExtra = extra.Subtotal,
                    EstadoReservaExtra = "ACT",
                    OrigenRegistro = "API",
                    CreadoPorUsuario = "API",
                    FechaRegistroUtc = DateTimeOffset.UtcNow
                };
                await _context.ReservaExtras.AddAsync(extraEntity);
            }
        }

        model.IdReserva = entity.IdReserva;
        model.ReservaGuid = entity.ReservaGuid;
        return model;
    }

    public async Task<bool> ExisteSolapamientoAsync(int idVehiculo, DateTimeOffset fechaInicio, DateTimeOffset fechaFin)
    {
        return await _context.Reservas
            .AnyAsync(r => r.IdVehiculo == idVehiculo
                && r.EstadoReserva != "CANCELADA"
                && r.EstadoReserva != "FINALIZADA"
                && r.EstadoReserva != "NO_SHOW"
                && r.FechaHoraRecogida < fechaFin
                && r.FechaHoraDevolucion > fechaInicio);
    }

    public async Task UpdateEstadoAsync(int id, string estado, string usuario, string? motivo = null)
    {
        var entity = await _context.Reservas.FindAsync(id);
        if (entity != null)
        {
            entity.EstadoReserva = estado;
            entity.ModificadoPorUsuario = usuario;
            entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
            if (estado == "CANCELADA")
            {
                entity.FechaCancelacionUtc = DateTimeOffset.UtcNow;
                entity.MotivoCancelacion = motivo;
            }
        }
    }

    public async Task AddConductorAsync(int idReserva, int idConductor, bool esPrincipal, decimal cargoConductorJoven)
    {
        var entity = new ReservaConductorEntity
        {
            ReservaConductorGuid = Guid.NewGuid(),
            IdReserva = idReserva,
            IdConductor = idConductor,
            TipoConductor = esPrincipal ? "PRINCIPAL" : "ADICIONAL",
            EsPrincipal = esPrincipal,
            CargoConductorJoven = cargoConductorJoven,
            EstadoReservaConductor = "ACT",
            OrigenRegistro = "API"
        };
        await _context.ReservaConductores.AddAsync(entity);
    }

    private static ReservaModel MapToModel(ReservaEntity r) => new()
    {
        IdReserva = r.IdReserva,
        ReservaGuid = r.ReservaGuid,
        CodigoReserva = r.CodigoReserva,
        IdCliente = r.IdCliente,
        IdVehiculo = r.IdVehiculo,
        IdLocalizacionRecogida = r.IdLocalizacionRecogida,
        IdLocalizacionDevolucion = r.IdLocalizacionDevolucion,
        CanalReserva = r.CanalReserva,
        FechaHoraRecogida = r.FechaHoraRecogida,
        FechaHoraDevolucion = r.FechaHoraDevolucion,
        Subtotal = r.Subtotal,
        ValorImpuestos = r.ValorImpuestos,
        ValorExtras = r.ValorExtras,
        CargoOneWay = r.CargoOneWay,
        Total = r.Total,
        CodigoConfirmacion = r.CodigoConfirmacion,
        EstadoReserva = r.EstadoReserva,
        NombreCliente = r.Cliente != null ? $"{r.Cliente.CliNombre1} {r.Cliente.CliApellido1}" : null,
        PlacaVehiculo = r.Vehiculo?.PlacaVehiculo,
        DescripcionVehiculo = r.Vehiculo?.Marca != null ? $"{r.Vehiculo.Marca.NombreMarca} {r.Vehiculo.ModeloVehiculo}" : r.Vehiculo?.ModeloVehiculo,
        Extras = r.Extras?.Where(e => e.EstadoReservaExtra == "ACT").Select(e => new ReservaExtraModel
        {
            IdReservaExtra = e.IdReservaExtra,
            IdExtra = e.IdExtra,
            CodigoExtra = e.Extra?.CodigoExtra ?? string.Empty,
            NombreExtra = e.Extra?.NombreExtra ?? string.Empty,
            Cantidad = e.Cantidad,
            ValorUnitario = e.ValorUnitarioExtra,
            Subtotal = e.SubtotalExtra
        }).ToList() ?? new()
    };
}
