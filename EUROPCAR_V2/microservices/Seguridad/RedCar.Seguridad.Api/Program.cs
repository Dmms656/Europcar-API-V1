using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RedCar.Seguridad.Api.Extensions;
using RedCar.Seguridad.Api.Grpc;
using RedCar.Seguridad.Business.Auth;
using RedCar.Seguridad.DataAccess.Context;
using RedCar.Shared.Auth;
using RedCar.Shared.Contracts.Common;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureKestrelForRestAndGrpc();

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default__Seguridad");
}
connectionString ??= string.Empty;

builder.Services.AddDbContext<SeguridadDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseNpgsql(connectionString, npg => npg.EnableRetryOnFailure(3));
    }
    else
    {
        // Fallback en memoria para que el MS pueda arrancar sin DB durante el bootstrap.
        // En cuanto se configure ConnectionStrings:Default este branch desaparece.
        options.UseInMemoryDatabase("RedCar.Seguridad.Dev");
    }
});

builder.Services.AddRedCarJwt(builder.Configuration);

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpc();

var healthBuilder = builder.Services.AddHealthChecks();
if (!string.IsNullOrWhiteSpace(connectionString))
{
    healthBuilder.AddDbContextCheck<SeguridadDbContext>("database", tags: new[] { "ready" });
}

var app = builder.Build();

var enableSwagger = app.Environment.IsDevelopment()
    || app.Configuration.GetValue("Swagger:Enabled", false);

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RedCar.Seguridad v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<SeguridadGrpcService>();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthResponseAsync
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponseAsync
});

app.MapGet("/", () => Results.Redirect("/info"));

app.Logger.LogInformation("RedCar.Seguridad iniciado en {Urls}", string.Join(",", app.Urls));
app.Run();


static Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json; charset=utf-8";
    var payload = new ApiResponse<object>
    {
        Success = report.Status == HealthStatus.Healthy,
        StatusCode = report.Status == HealthStatus.Healthy ? 200 : 503,
        Message = report.Status.ToString(),
        Data = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds,
                error = e.Value.Exception?.Message
            })
        },
        TraceId = context.TraceIdentifier
    };
    var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    });
    return context.Response.WriteAsync(json);
}
