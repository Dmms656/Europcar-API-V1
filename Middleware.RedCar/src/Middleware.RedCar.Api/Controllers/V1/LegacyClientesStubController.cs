using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Middleware.RedCar.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/Clientes")]
[Produces("application/json")]
public sealed class LegacyClientesStubController : ControllerBase
{
    private object Envelope(object data) => new
    {
        success = true,
        statusCode = 200,
        message = "OK (stub: sin datos hasta MS Clientes)",
        data,
        traceId = HttpContext.TraceIdentifier
    };

    [HttpGet]
    public IActionResult GetAll() => Ok(Envelope(Array.Empty<object>()));

    [HttpGet("{id:int}")]
    public IActionResult GetById([FromRoute] int id) => NotFound(new { success = false, statusCode = 404, message = "Cliente no encontrado (stub).", data = (object?)null });

    [HttpPost]
    public IActionResult Create() => StatusCode(501, new { success = false, statusCode = 501, message = "Alta no implementada (stub).", data = (object?)null });

    [HttpPut("{id:int}")]
    public IActionResult Update([FromRoute] int id) => StatusCode(501, new { success = false, statusCode = 501, message = "Actualización no implementada (stub).", data = (object?)null });

    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id) => StatusCode(501, new { success = false, statusCode = 501, message = "Baja no implementada (stub).", data = (object?)null });
}
