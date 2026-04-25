namespace Europcar.Rental.Business.Exceptions;

public class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message) : base(message, 401) { }
}
