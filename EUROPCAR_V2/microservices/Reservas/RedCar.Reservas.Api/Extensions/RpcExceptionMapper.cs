using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Reservas.Api.Extensions;

internal static class RpcExceptionMapper
{
    public static ActionResult<ApiResponse<T>> ToActionResult<T>(RpcException ex, HttpContext httpContext)
    {
        var traceId = httpContext.TraceIdentifier;
        var detail = string.IsNullOrWhiteSpace(ex.Status.Detail) ? ex.Status.StatusCode.ToString() : ex.Status.Detail;

        return ex.StatusCode switch
        {
            StatusCode.InvalidArgument => new BadRequestObjectResult(
                ApiResponse<T>.Fail(400, detail, traceId)),
            StatusCode.NotFound => new NotFoundObjectResult(
                ApiResponse<T>.Fail(404, detail, traceId)),
            StatusCode.FailedPrecondition => new ConflictObjectResult(
                ApiResponse<T>.Fail(409, detail, traceId)),
            StatusCode.AlreadyExists => new ConflictObjectResult(
                ApiResponse<T>.Fail(409, detail, traceId)),
            _ => new ObjectResult(ApiResponse<T>.Fail(500, detail, traceId)) { StatusCode = 500 }
        };
    }
}
