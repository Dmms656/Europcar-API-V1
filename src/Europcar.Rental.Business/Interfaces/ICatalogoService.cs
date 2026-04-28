using Europcar.Rental.Business.DTOs.Request.Auth;
using Europcar.Rental.Business.DTOs.Request.Clientes;
using Europcar.Rental.Business.DTOs.Request.Vehiculos;
using Europcar.Rental.Business.DTOs.Request.Reservas;
using Europcar.Rental.Business.DTOs.Request.Contratos;
using Europcar.Rental.Business.DTOs.Request.Pagos;
using Europcar.Rental.Business.DTOs.Request.Mantenimientos;
using Europcar.Rental.Business.DTOs.Request.Booking;
using Europcar.Rental.Business.DTOs.Request.Catalogos;
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

public interface ICatalogoService
{
    Task<IEnumerable<LocalizacionResponse>> GetLocalizacionesAsync();
    Task<LocalizacionResponse> GetLocalizacionByIdAsync(int id);
    Task<IEnumerable<CatalogoResponse>> GetCategoriasAsync();
    Task<IEnumerable<CatalogoResponse>> GetMarcasAsync();
    Task<IEnumerable<ExtraResponse>> GetExtrasAsync();
    Task<ExtraResponse> GetExtraByIdAsync(int id);
    Task<ExtraResponse> CreateExtraAsync(CrearExtraRequest request, string usuario);
    Task<ExtraResponse> UpdateExtraAsync(int id, ActualizarExtraRequest request, string usuario);
    Task CambiarEstadoExtraAsync(int id, CambiarEstadoExtraRequest request, string usuario);
    Task DeleteExtraAsync(int id, string usuario);
}
