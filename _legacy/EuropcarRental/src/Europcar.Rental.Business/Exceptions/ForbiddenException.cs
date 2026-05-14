namespace Europcar.Rental.Business.Exceptions;

public class ForbiddenException : BusinessException
{
    public ForbiddenException(string message) : base(message, 403) { }
}
