namespace Middleware.RedCar.Business.Exceptions;

/// <summary>404 - Recurso no encontrado.</summary>
public sealed class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message, 404) { }
}
