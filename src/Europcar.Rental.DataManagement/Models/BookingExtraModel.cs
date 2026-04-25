namespace Europcar.Rental.DataManagement.Models;

/// <summary>
/// Modelo de extra con precio fijo para las respuestas de Booking / OTA.
/// </summary>
public class BookingExtraModel
{
    public int IdExtra { get; set; }
    public string CodigoExtra { get; set; } = string.Empty;
    public string NombreExtra { get; set; } = string.Empty;
    public string? DescripcionExtra { get; set; }
    public decimal ValorFijo { get; set; }
}
