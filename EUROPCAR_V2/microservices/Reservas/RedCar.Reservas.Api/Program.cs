using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RedCar.Reservas.Api.Extensions;
using RedCar.Reservas.Api.Grpc;
using RedCar.Reservas.DataAccess.Context;
using RedCar.Shared.Auth;
using RedCar.Shared.Contracts.Common;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureKestrelForRestAndGrpc();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default__Reservas")
    ?? string.Empty;

builder.Services.AddDbContext<ReservasDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseNpgsql(connectionString, npg => npg.EnableRetryOnFailure(3));
    }
    else
    {
        options.UseInMemoryDatabase("RedCar.Reservas.Dev");
    }
});

builder.Services.AddRedCarJwt(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();

var healthBuilder = builder.Services.AddHealthChecks();
if (!string.IsNullOrWhiteSpace(connectionString))
{
    healthBuilder.AddDbContextCheck<ReservasDbContext>("database", tags: new[] { "ready" });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RedCar.Reservas v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<ReservasGrpcService>();

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

app.Logger.LogInformation("RedCar.Reservas iniciado en {Urls}", string.Join(",", app.Urls));
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
