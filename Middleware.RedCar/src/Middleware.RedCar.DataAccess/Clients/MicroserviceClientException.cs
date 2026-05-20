using System.Net;

namespace Middleware.RedCar.DataAccess.Clients;

/// <summary>
/// Error HTTP devuelto por un microservicio downstream (body ApiResponse con statusCode).
/// </summary>
public sealed class MicroserviceClientException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public MicroserviceClientException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
