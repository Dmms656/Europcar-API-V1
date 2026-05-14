namespace Europcar.Rental.Business.Exceptions;

public class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message, 404) { }
}
