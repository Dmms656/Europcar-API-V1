using Europcar.Rental.Business.DTOs.Request.Auth;
using Europcar.Rental.Business.DTOs.Request.Clientes;
using Europcar.Rental.Business.DTOs.Request.Vehiculos;
using Europcar.Rental.Business.DTOs.Request.Reservas;
using Europcar.Rental.Business.DTOs.Request.Contratos;
using Europcar.Rental.Business.DTOs.Request.Pagos;
using Europcar.Rental.Business.DTOs.Request.Mantenimientos;
using Europcar.Rental.Business.DTOs.Request.Booking;
using Europcar.Rental.Business.DTOs.Response.Auth;
using Europcar.Rental.Business.DTOs.Response.Clientes;
using Europcar.Rental.Business.DTOs.Response.Vehiculos;
using Europcar.Rental.Business.DTOs.Response.Reservas;
using Europcar.Rental.Business.DTOs.Response.Contratos;
using Europcar.Rental.Business.DTOs.Response.Pagos;
using Europcar.Rental.Business.DTOs.Response.Mantenimientos;
using Europcar.Rental.Business.DTOs.Response.Catalogos;
using Europcar.Rental.Business.DTOs.Response.Booking;

namespace Europcar.Rental.Business.Interfaces;

/// <summary>
/// Servicio de negocio para los endpoints públicos de Booking / OTA.
/// Implementa los 7 endpoints del contrato de API externo.
/// </summary>
public interface IBookingService
{
    // Endpoint 1: Búsqueda paginada de vehículos disponibles
    Task<BookingResponse<BookingVehiculoListData>> BuscarVehiculosAsync(BookingBuscarVehiculosRequest request);

    // Endpoint 2: Detalle completo de un vehículo
    Task<BookingResponse<BookingVehiculoDetailData>> GetVehiculoDetalleAsync(int vehiculoId);

    // Endpoint 3: Verificar disponibilidad en tiempo real
    Task<BookingResponse<BookingDisponibilidadCheckData>> VerificarDisponibilidadAsync(int vehiculoId, BookingDisponibilidadRequest request);

    // Endpoint 4: Listar localizaciones con paginación
    Task<BookingResponse<BookingLocalizacionListData>> GetLocalizacionesAsync(BookingLocalizacionesRequest request);

    // Endpoint 5: Detalle de una localización
    Task<BookingResponse<BookingLocalizacionDetailData>> GetLocalizacionDetalleAsync(int localizacionId);

    // Endpoint 6: Listar categorías
    Task<BookingResponse<BookingCategoriaListData>> GetCategoriasAsync();

    // Endpoint 7: Listar extras disponibles
    Task<BookingResponse<BookingExtraListData>> GetExtrasAsync();
}
