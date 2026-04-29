using System.Text.Json;
using Europcar.Rental.Api.Extensions;
using Europcar.Rental.Api.Middleware;
using Europcar.Rental.Api.Models.Common;
using Europcar.Rental.Business.Services;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Security;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// === Servicios ===
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy();
builder.Services.AddHealthChecksConfiguration();
builder.Services.AddControllers();
builder.Services.AddApiBehavior();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// === Pipeline ===
// El middleware global de errores debe ir lo más arriba posible para
// capturar excepciones de cualquier middleware/endpoint posterior.
app.UseMiddleware<GlobalExceptionMiddleware>();

// === Seed inicial (resiliente y aislado) ===
await SeedDatabaseAsync(app);

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Europcar Rental API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// === Health checks ===
// /health/live  -> proceso vivo (responde rápido sin dependencias)
// /health/ready -> dependencias OK (DB)
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

app.Logger.LogInformation("🚗 Europcar Rental API iniciada. Swagger: /swagger");
app.Run();


// ============================================================================
// Helpers locales
// ============================================================================

static async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var db = sp.GetRequiredService<RentalDbContext>();

    try
    {
        // EnableRetryOnFailure ya cubre transitorios; aquí logueamos nivel alto.
        await SeedUserIfMissingAsync(db, "admin.dev", "admin@europcar.dev", "ADMIN", logger);
        await SeedUserIfMissingAsync(db, "agente.pos", "agente@europcar.dev", "AGENTE_POS", logger);
        await SeedUserIfMissingAsync(db, "cliente.web", "cliente@europcar.dev", "CLIENTE_WEB", logger);
        await ApplyDatabaseHotfixesAsync(db, logger);
    }
    catch (Exception ex)
    {
        // No queremos que un fallo del seed tumbe la API; sólo lo reportamos.
        logger.LogWarning(ex, "Seed inicial falló: {Message}", ex.Message);
    }
}

static async Task SeedUserIfMissingAsync(
    RentalDbContext db,
    string username,
    string correo,
    string rol,
    ILogger logger)
{
    var existing = await db.UsuariosApp
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Username == username);

    if (existing != null)
    {
        // Reparación para ambientes donde el usuario fue creado con otro esquema de hash (p.ej. pgcrypto).
        if (!LooksLikeBase64(existing.PasswordSalt) || !LooksLikeBase64(existing.PasswordHash))
        {
            var (repairHash, repairSalt) = AuthService.CreatePasswordHash("12345");
            existing.PasswordHash = repairHash;
            existing.PasswordSalt = repairSalt;
            existing.RequiereCambioPassword = false;
            existing.EstadoUsuario = "ACT";
            existing.Activo = true;
            existing.ModificadoPorUsuario = "SYSTEM";
            existing.FechaModificacionUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            logger.LogWarning("Seed: credenciales de '{Username}' reparadas al esquema HMAC local.", username);
        }

        return;
    }

    var (hash, salt) = AuthService.CreatePasswordHash("12345");

    var user = new UsuarioAppEntity
    {
        UsuarioGuid = Guid.NewGuid(),
        Username = username,
        Correo = correo,
        PasswordHash = hash,
        PasswordSalt = salt,
        RequiereCambioPassword = false,
        EstadoUsuario = "ACT",
        Activo = true,
        CreadoPorUsuario = "SYSTEM",
        FechaRegistroUtc = DateTimeOffset.UtcNow
    };
    db.UsuariosApp.Add(user);
    await db.SaveChangesAsync();

    var rolEntity = await db.Roles
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(r => r.NombreRol == rol);

    if (rolEntity != null)
    {
        db.UsuariosRoles.Add(new UsuarioRolEntity
        {
            IdUsuario = user.IdUsuario,
            IdRol = rolEntity.IdRol,
            EstadoUsuarioRol = "ACT",
            Activo = true,
            CreadoPorUsuario = "SYSTEM",
            FechaRegistroUtc = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }
    else
    {
        logger.LogWarning("Seed: rol '{Rol}' no existe; usuario '{Username}' creado sin rol.", rol, username);
    }

    logger.LogInformation("Seed: usuario '{Username}' creado con contraseña por defecto.", username);
}

static bool LooksLikeBase64(string? value)
{
    if (string.IsNullOrWhiteSpace(value)) return false;
    Span<byte> buffer = new byte[value.Length];
    return Convert.TryFromBase64String(value, buffer, out _);
}

static async Task ApplyDatabaseHotfixesAsync(RentalDbContext db, ILogger logger)
{
    // Hotfix: cancelar/finalizar una reserva no debe volver a validar disponibilidad del vehículo.
    // Sin este bypass, el trigger de validación puede bloquear cancelaciones legítimas.
    const string sql = """
CREATE OR REPLACE FUNCTION rental.fn_validar_reserva()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_estado_vehiculo VARCHAR(20);
    v_fecha_nacimiento DATE;
BEGIN
    -- En cambios de estado no operativos (cancelar/finalizar/no_show),
    -- no revalidar disponibilidad ni solapamientos.
    IF TG_OP = 'UPDATE'
       AND NEW.estado_reserva IN ('CANCELADA', 'FINALIZADA', 'NO_SHOW')
       AND NEW.id_vehiculo = OLD.id_vehiculo
       AND NEW.fecha_hora_recogida = OLD.fecha_hora_recogida
       AND NEW.fecha_hora_devolucion = OLD.fecha_hora_devolucion THEN
        NEW.total := COALESCE(NEW.subtotal, 0)
                   + COALESCE(NEW.valor_impuestos, 0)
                   + COALESCE(NEW.valor_extras, 0)
                   + COALESCE(NEW.cargo_one_way, 0);
        RETURN NEW;
    END IF;

    IF rental.fn_reservas_solapadas(
        NEW.id_vehiculo,
        NEW.fecha_hora_recogida,
        NEW.fecha_hora_devolucion,
        COALESCE(NEW.id_reserva, NULL)
    ) THEN
        RAISE EXCEPTION 'El vehículo % ya tiene una reserva activa en el rango solicitado', NEW.id_vehiculo;
    END IF;

    SELECT estado_operativo INTO v_estado_vehiculo
    FROM rental.vehiculos
    WHERE id_vehiculo = NEW.id_vehiculo;

    IF v_estado_vehiculo IN ('MANTENIMIENTO', 'TALLER', 'ALQUILADO', 'FUERA_SERVICIO') THEN
        RAISE EXCEPTION 'El vehículo % no está disponible. Estado operativo actual: %', NEW.id_vehiculo, v_estado_vehiculo;
    END IF;

    SELECT fecha_nacimiento INTO v_fecha_nacimiento
    FROM rental.clientes
    WHERE id_cliente = NEW.id_cliente;

    IF age(NEW.fecha_hora_recogida::date, v_fecha_nacimiento) < INTERVAL '21 years' THEN
        RAISE EXCEPTION 'El cliente % no cumple la edad mínima de 21 años para alquilar', NEW.id_cliente;
    END IF;

    NEW.total := COALESCE(NEW.subtotal, 0)
               + COALESCE(NEW.valor_impuestos, 0)
               + COALESCE(NEW.valor_extras, 0)
               + COALESCE(NEW.cargo_one_way, 0);

    RETURN NEW;
END;
$$;
""";

    await db.Database.ExecuteSqlRawAsync(sql);
    logger.LogInformation("Hotfix BD aplicado: fn_validar_reserva ajustada para cancelaciones.");
}

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
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
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
