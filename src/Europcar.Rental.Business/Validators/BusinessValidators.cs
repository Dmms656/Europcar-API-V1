using Europcar.Rental.Business.DTOs.Request.Auth;
using Europcar.Rental.Business.DTOs.Request.Clientes;
using Europcar.Rental.Business.DTOs.Request.Vehiculos;
using Europcar.Rental.Business.DTOs.Request.Reservas;
using Europcar.Rental.Business.Exceptions;

namespace Europcar.Rental.Business.Validators;

/// <summary>
/// Validadores reutilizables de reglas de negocio.
/// Lanzan BusinessException cuando la validación falla.
/// </summary>
public static class ClienteValidator
{
    private static readonly HashSet<string> TiposIdentificacionValidos = new() { "DNI", "PAS", "RUC", "CED" };

    public static void ValidarCreacion(CrearClienteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre1))
            throw new BusinessException("El primer nombre es obligatorio");

        if (string.IsNullOrWhiteSpace(request.Apellido1))
            throw new BusinessException("El primer apellido es obligatorio");

        if (string.IsNullOrWhiteSpace(request.NumeroIdentificacion))
            throw new BusinessException("El número de identificación es obligatorio");

        if (!TiposIdentificacionValidos.Contains(request.TipoIdentificacion.ToUpper()))
            throw new BusinessException($"Tipo de identificación inválido. Valores permitidos: {string.Join(", ", TiposIdentificacionValidos)}");

        ValidarEdadMinima(request.FechaNacimiento, 18);

        if (string.IsNullOrWhiteSpace(request.Correo) || !request.Correo.Contains('@'))
            throw new BusinessException("El correo electrónico no es válido");

        if (string.IsNullOrWhiteSpace(request.Telefono))
            throw new BusinessException("El teléfono es obligatorio");
    }

    public static void ValidarActualizacion(ActualizarClienteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre1))
            throw new BusinessException("El primer nombre es obligatorio");

        if (string.IsNullOrWhiteSpace(request.Apellido1))
            throw new BusinessException("El primer apellido es obligatorio");

        if (!TiposIdentificacionValidos.Contains(request.TipoIdentificacion.ToUpper()))
            throw new BusinessException($"Tipo de identificación inválido. Valores permitidos: {string.Join(", ", TiposIdentificacionValidos)}");

        ValidarEdadMinima(request.FechaNacimiento, 18);

        if (request.RowVersion <= 0)
            throw new BusinessException("Se requiere RowVersion para control de concurrencia");
    }

    private static void ValidarEdadMinima(DateOnly fechaNacimiento, int edadMinima)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var edad = hoy.Year - fechaNacimiento.Year;
        if (fechaNacimiento > hoy.AddYears(-edad)) edad--;

        if (edad < edadMinima)
            throw new BusinessException($"El cliente debe tener al menos {edadMinima} años");
    }
}

public static class ReservaValidator
{
    private static readonly HashSet<string> CanalesValidos = new() { "WEB", "POS", "BOOKING", "APP", "BACKOFFICE", "API" };

    public static void ValidarCreacion(CrearReservaRequest request)
    {
        if (request.FechaHoraDevolucion <= request.FechaHoraRecogida)
            throw new BusinessException("La fecha de devolución debe ser posterior a la de recogida");

        if (request.FechaHoraRecogida <= DateTimeOffset.UtcNow)
            throw new BusinessException("La fecha de recogida debe ser futura");

        if (request.IdCliente <= 0)
            throw new BusinessException("El ID del cliente es obligatorio");

        if (request.IdVehiculo <= 0)
            throw new BusinessException("El ID del vehículo es obligatorio");

        if (request.IdLocalizacionRecogida <= 0)
            throw new BusinessException("La localización de recogida es obligatoria");

        if (request.IdLocalizacionDevolucion <= 0)
            throw new BusinessException("La localización de devolución es obligatoria");

        if (!string.IsNullOrWhiteSpace(request.CanalReserva) && !CanalesValidos.Contains(request.CanalReserva.ToUpper()))
            throw new BusinessException($"Canal de reserva inválido. Valores permitidos: {string.Join(", ", CanalesValidos)}");
    }
}

public static class LoginValidator
{
    public static void Validar(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new BusinessException("El nombre de usuario es obligatorio");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BusinessException("La contraseña es obligatoria");
    }
}
