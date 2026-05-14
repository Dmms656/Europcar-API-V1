namespace Middleware.RedCar.Business.Exceptions;

/// <summary>
/// Base de las excepciones de negocio del middleware. Se traduce a HTTP
/// en ExceptionHandlingMiddleware segun el HttpStatusCode asociado.
/// </summary>
public class BusinessException : Exception
{
    public int HttpStatusCode { get; }

    public BusinessException(string message, int httpStatusCode = 500) : base(message)
    {
        HttpStatusCode = httpStatusCode;
    }

    public BusinessException(string message, Exception inner, int httpStatusCode = 500) : base(message, inner)
    {
        HttpStatusCode = httpStatusCode;
    }
}
