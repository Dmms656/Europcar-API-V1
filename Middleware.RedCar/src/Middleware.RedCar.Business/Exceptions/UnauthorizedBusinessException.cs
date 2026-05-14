namespace Middleware.RedCar.Business.Exceptions;

/// <summary>401 - Token invalido o ausente.</summary>
public sealed class UnauthorizedBusinessException : BusinessException
{
    public UnauthorizedBusinessException(string message) : base(message, 401) { }
}
