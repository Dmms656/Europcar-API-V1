namespace Middleware.RedCar.Api.Compatibility;

/// <summary>
/// Misma forma que el monolito <c>BookingResponse&lt;T&gt;</c>: status, mensaje, data.
/// El frontend lee <c>response.data.data</c> (axios).
/// </summary>
public static class LegacyBookingEnvelope
{
    public static object Ok(object data, string mensaje = "Operación exitosa", int status = 200) =>
        new { status, mensaje, data };

    public static object Created(object data, string mensaje = "Reserva creada exitosamente") =>
        new { status = 201, mensaje, data };

    public static object Fail(string mensaje, int status = 400) =>
        new { status, mensaje, data = (object?)null };
}
