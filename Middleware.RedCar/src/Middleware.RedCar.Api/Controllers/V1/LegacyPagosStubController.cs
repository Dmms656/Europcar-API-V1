using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedCar.Shared.Contracts.Common;

namespace Middleware.RedCar.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/Pagos")]
public sealed class LegacyPagosStubController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(ApiResponse<object>.Ok(Array.Empty<object>(), traceId: HttpContext.TraceIdentifier));

    [HttpPost]
    [HttpPut("{id:int}")]
    public IActionResult Write() => StatusCode(501, ApiResponse<object>.Fail(501, "Módulo de pagos no migrado aún.", HttpContext.TraceIdentifier));
}
