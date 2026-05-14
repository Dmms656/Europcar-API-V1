using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Middleware.RedCar.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/Contratos")]
[Produces("application/json")]
public sealed class LegacyContratosStubController : ControllerBase
{
    private object Envelope(object data) => new
    {
        success = true,
        statusCode = 200,
        message = "OK (stub: sin datos hasta MS Reservas/Contratos)",
        data,
        traceId = HttpContext.TraceIdentifier
    };

    [HttpGet]
    public IActionResult GetAll() => Ok(Envelope(Array.Empty<object>()));

    [HttpGet("mis-contratos")]
    public IActionResult MisContratos() => Ok(Envelope(Array.Empty<object>()));

    [HttpGet("{id:int}")]
    public IActionResult GetById([FromRoute] int id) => NotFound(new { success = false, statusCode = 404, message = "Contrato no encontrado (stub).", data = (object?)null });

    [HttpPost]
    public IActionResult Create() => StatusCode(501, new { success = false, statusCode = 501, message = "Alta no implementada (stub).", data = (object?)null });

    [HttpPut("{id:int}")]
    public IActionResult Update([FromRoute] int id) => StatusCode(501, new { success = false, statusCode = 501, message = "Actualización no implementada (stub).", data = (object?)null });

    [HttpPost("checkout")]
    public IActionResult Checkout() => StatusCode(501, new { success = false, statusCode = 501, message = "Checkout no implementado (stub).", data = (object?)null });

    [HttpPost("checkin")]
    public IActionResult Checkin() => StatusCode(501, new { success = false, statusCode = 501, message = "Checkin no implementado (stub).", data = (object?)null });
}
