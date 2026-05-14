namespace Middleware.RedCar.Business.Exceptions;

/// <summary>409 - Conflicto (ej: vehiculo ya no disponible para esas fechas).</summary>
public sealed class ConflictException : BusinessException
{
    public ConflictException(string message) : base(message, 409) { }
}
