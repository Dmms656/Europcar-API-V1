using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Services;

/// <summary>
/// Servicio de datos especializado para el módulo Booking / OTA.
/// Provee extras con precio fijo y localizaciones con Id de ciudad.
/// </summary>
public class BookingDataService : IBookingDataService
{
    private readonly RentalDbContext _context;

    public BookingDataService(RentalDbContext context) => _context = context;

    public async Task<IEnumerable<BookingExtraModel>> GetExtrasConPrecioAsync()
    {
        return await _context.Extras
            .Where(e => e.EstadoExtra == "ACT")
            .Select(e => new BookingExtraModel
            {
                IdExtra = e.IdExtra,
                CodigoExtra = e.CodigoExtra,
                NombreExtra = e.NombreExtra,
                DescripcionExtra = e.DescripcionExtra,
                ValorFijo = e.ValorFijo
            }).ToListAsync();
    }

    public async Task<IEnumerable<BookingLocalizacionModel>> GetLocalizacionesConCiudadIdAsync()
    {
        return await _context.Localizaciones
            .Include(l => l.Ciudad)
            .Where(l => l.EstadoLocalizacion == "ACT")
            .Select(l => new BookingLocalizacionModel
            {
                IdLocalizacion = l.IdLocalizacion,
                CodigoLocalizacion = l.CodigoLocalizacion,
                NombreLocalizacion = l.NombreLocalizacion,
                DireccionLocalizacion = l.DireccionLocalizacion,
                TelefonoContacto = l.TelefonoContacto,
                CorreoContacto = l.CorreoContacto,
                HorarioAtencion = l.HorarioAtencion,
                ZonaHoraria = l.ZonaHoraria,
                IdCiudad = l.IdCiudad,
                NombreCiudad = l.Ciudad.NombreCiudad
            }).ToListAsync();
    }

    public async Task<BookingLocalizacionModel?> GetLocalizacionConCiudadIdAsync(int id)
    {
        var l = await _context.Localizaciones
            .Include(l => l.Ciudad)
            .FirstOrDefaultAsync(l => l.IdLocalizacion == id && l.EstadoLocalizacion == "ACT");

        if (l == null) return null;

        return new BookingLocalizacionModel
        {
            IdLocalizacion = l.IdLocalizacion,
            CodigoLocalizacion = l.CodigoLocalizacion,
            NombreLocalizacion = l.NombreLocalizacion,
            DireccionLocalizacion = l.DireccionLocalizacion,
            TelefonoContacto = l.TelefonoContacto,
            CorreoContacto = l.CorreoContacto,
            HorarioAtencion = l.HorarioAtencion,
            ZonaHoraria = l.ZonaHoraria,
            IdCiudad = l.IdCiudad,
            NombreCiudad = l.Ciudad.NombreCiudad
        };
    }

    public async Task<BookingFacturaModel?> GetFacturaByReservaCodigoAsync(string codigoReserva)
    {
        if (string.IsNullOrWhiteSpace(codigoReserva)) return null;

        return await _context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Reserva)
            .Where(f => f.Reserva != null && f.Reserva.CodigoReserva == codigoReserva)
            .OrderByDescending(f => f.FechaEmision)
            .Select(f => new BookingFacturaModel
            {
                IdFactura = f.IdFactura,
                NumeroFactura = f.NumeroFactura,
                CodigoReserva = f.Reserva!.CodigoReserva,
                EstadoFactura = f.EstadoFactura,
                FechaEmision = f.FechaEmision,
                Subtotal = f.Subtotal,
                ValorIva = f.ValorIva,
                Total = f.Total,
                ClienteNombres = (f.Cliente.CliNombre1 + " " + (f.Cliente.CliNombre2 ?? "")).Trim(),
                ClienteApellidos = (f.Cliente.CliApellido1 + " " + (f.Cliente.CliApellido2 ?? "")).Trim(),
                ClienteIdentificacion = f.Cliente.NumeroIdentificacion
            })
            .FirstOrDefaultAsync();
    }
}
