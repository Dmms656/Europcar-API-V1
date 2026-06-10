using System.Text.Json;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Extensions;
using Middleware.RedCar.Api.Middleware;
using Middleware.RedCar.Api.Models.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Las variables de entorno con __ se mapean a secciones de configuracion
// (ya viene activado por defecto via AddEnvironmentVariables()).

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Cuando ASP.NET detecta un ModelState invalido (campos faltantes, parseo fallido,
        // [Required] no cumplido) responde 400. Aqui le damos la forma del contrato.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errores = context.ModelState
                .Where(kv => kv.Value is not null && kv.Value.Errors.Count > 0)
                .SelectMany(kv => kv.Value!.Errors.Select(e => new ApiFieldError(kv.Key, e.ErrorMessage)))
                .ToList();

            var errorsDict = errores.ToDictionary(e => e.Campo, e => new[] { e.Mensaje });
            var payload = new ApiErrorResponse
            {
                Status = 400,
                StatusCode = 400,
                Success = false,
                Mensaje = "Parametros invalidos o faltantes.",
                Message = "Parametros invalidos o faltantes.",
                TraceId = context.HttpContext.TraceIdentifier,
                Errores = errores,
                Errors = errorsDict
            };
            return new BadRequestObjectResult(payload);
        };
    })
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddRedCarApiVersioning();
builder.Services.AddRedCarCors(builder.Configuration);
builder.Services.AddEmbeddedSeguridadAuth(builder.Configuration);
builder.Services.AddRedCarAuthentication(builder.Configuration);

builder.Services.AddMicroservicioHttpClients();
builder.Services.AddRedCarGrpcClients();
builder.Services.AddGraphQlIntegrationClient(builder.Configuration);
builder.Services.AddRedCarEventBus(builder.Configuration);
builder.Services.AddRedCarMiddlewareServices(builder.Configuration);

builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Middleware.RedCar — Compatibilidad frontend",
        Version = "v1.0",
        Description = "Rutas publicas /api/v1/* equivalentes al monolito Europcar.Rental.Api (bookingApi.js)."
    });

    c.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Middleware.RedCar",
        Version = "v2.0.0",
        Description = "Implementacion del Contrato API de Vehiculos RedCar V2."
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT en el header Authorization. Ej: Bearer eyJ...",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var group = apiDesc.GroupName ?? string.Empty;
        if (string.Equals(docName, "v1", StringComparison.OrdinalIgnoreCase))
            return group.StartsWith("v1", StringComparison.OrdinalIgnoreCase);
        if (string.Equals(docName, "v2", StringComparison.OrdinalIgnoreCase))
            return group.StartsWith("v2", StringComparison.OrdinalIgnoreCase);
        return true;
    });
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<ExceptionHandlingMiddleware>();

var enableSwagger = app.Environment.IsDevelopment()
    || app.Configuration.GetValue("Swagger:Enabled", false);

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1 — Frontend (bookingApi)");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "v2 — Contrato RedCar");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors(CorsExtensions.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapGet("/", () => enableSwagger
    ? Results.Redirect("/swagger")
    : Results.Redirect("/health/live"));

app.Logger.LogInformation("Middleware.RedCar V2 iniciado en {Urls}", string.Join(",", app.Urls));
app.Run();
