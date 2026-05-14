namespace Middleware.RedCar.Business.DTOs.Booking;

/// <summary>
/// Bloque "paginacion" identico en todos los endpoints de listado del contrato.
/// </summary>
public sealed class PaginacionResponse
{
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalElementos { get; set; }
    public int ElementosPorPagina { get; set; }
}
