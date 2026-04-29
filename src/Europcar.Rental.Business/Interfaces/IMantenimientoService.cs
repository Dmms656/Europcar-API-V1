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

public interface IMantenimientoService
{
    Task<IEnumerable<MantenimientoResponse>> GetAllAsync();
    Task<MantenimientoResponse> GetByIdAsync(int id);
    Task<IEnumerable<MantenimientoResponse>> GetByVehiculoIdAsync(int idVehiculo);
    Task<MantenimientoResponse> CreateAsync(CrearMantenimientoRequest request, string usuario);
    Task<MantenimientoResponse> UpdateAsync(int id, ActualizarMantenimientoRequest request, string usuario);
    Task<MantenimientoResponse> CerrarAsync(int id, string usuario);
}
