namespace Middleware.RedCar.Business.DTOs.Booking;

/// <summary>
/// Item del Endpoint 6 (GET /api/v2/booking/categorias). Forma exacta del contrato.
/// </summary>
public sealed class CategoriaBookingResponse
{
    public int IdCategoria { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
