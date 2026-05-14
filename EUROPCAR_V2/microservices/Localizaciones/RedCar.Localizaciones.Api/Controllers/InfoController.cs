using Microsoft.AspNetCore.Mvc;
using RedCar.Shared.Contracts.Common;

namespace RedCar.Localizaciones.Api.Controllers;

[ApiController]
[Route("info")]
public sealed class InfoController : ControllerBase
{
    private static readonly DateTimeOffset StartedAtUtc = DateTimeOffset.UtcNow;

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public InfoController(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    [HttpGet]
    public ActionResult<ApiResponse<ServiceInfo>> Get()
    {
        var info = new ServiceInfo
        {
            Service = _configuration["Service:Name"] ?? "RedCar.Localizaciones",
            Schema  = _configuration["Service:Schema"] ?? "localizaciones",
            Version = typeof(InfoController).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            Environment = _env.EnvironmentName,
            StartedAtUtc = StartedAtUtc,
            Status = "running"
        };
        return Ok(ApiResponse<ServiceInfo>.Ok(info, traceId: HttpContext.TraceIdentifier));
    }
}
