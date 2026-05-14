namespace Middleware.RedCar.DataManagement.Models.Reservas;

public sealed record FacturaDataModel(
    string NumeroFactura,
    string CodigoReserva,
    DateTimeOffset FechaFacturaUtc,
    decimal Subtotal,
    decimal Iva,
    decimal Total,
    string Moneda,
    string? UrlPdf);
