using Europcar.Rental.Api.Extensions;
using Europcar.Rental.Api.Middleware;
using Europcar.Rental.Business.Services;
using Europcar.Rental.DataAccess.Context;
using Europcar.Rental.DataAccess.Entities.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// === Servicios ===
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// === Seed del usuario admin.dev si no existe ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RentalDbContext>();
    try
    {
        var adminExists = await db.UsuariosApp
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username == "admin.dev");

        if (!adminExists)
        {
            var (hash, salt) = AuthService.CreatePasswordHash("12345");
            
            var adminUser = new UsuarioAppEntity
            {
                UsuarioGuid = Guid.NewGuid(),
                Username = "admin.dev",
                Correo = "admin@europcar.dev",
                PasswordHash = hash,
                PasswordSalt = salt,
                RequiereCambioPassword = false,
                EstadoUsuario = "ACT",
                Activo = true,
                CreadoPorUsuario = "SYSTEM",
                FechaRegistroUtc = DateTimeOffset.UtcNow
            };
            db.UsuariosApp.Add(adminUser);
            await db.SaveChangesAsync();

            // Asignar rol ADMIN
            var adminRol = await db.Roles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.NombreRol == "ADMIN");
            if (adminRol != null)
            {
                db.UsuariosRoles.Add(new UsuarioRolEntity
                {
                    IdUsuario = adminUser.IdUsuario,
                    IdRol = adminRol.IdRol,
                    EstadoUsuarioRol = "ACT",
                    Activo = true,
                    CreadoPorUsuario = "SYSTEM",
                    FechaRegistroUtc = DateTimeOffset.UtcNow
                });
                await db.SaveChangesAsync();
            }

            Console.WriteLine("✅ Usuario admin.dev creado con contraseña '12345'");
        }

        // Seed usuario agente.pos
        var agenteExists = await db.UsuariosApp
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username == "agente.pos");

        if (!agenteExists)
        {
            var (hash, salt) = AuthService.CreatePasswordHash("12345");
            
            var agenteUser = new UsuarioAppEntity
            {
                UsuarioGuid = Guid.NewGuid(),
                Username = "agente.pos",
                Correo = "agente@europcar.dev",
                PasswordHash = hash,
                PasswordSalt = salt,
                RequiereCambioPassword = false,
                EstadoUsuario = "ACT",
                Activo = true,
                CreadoPorUsuario = "SYSTEM",
                FechaRegistroUtc = DateTimeOffset.UtcNow
            };
            db.UsuariosApp.Add(agenteUser);
            await db.SaveChangesAsync();

            var agenteRol = await db.Roles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.NombreRol == "AGENTE_POS");
            if (agenteRol != null)
            {
                db.UsuariosRoles.Add(new UsuarioRolEntity
                {
                    IdUsuario = agenteUser.IdUsuario,
                    IdRol = agenteRol.IdRol,
                    EstadoUsuarioRol = "ACT",
                    Activo = true,
                    CreadoPorUsuario = "SYSTEM",
                    FechaRegistroUtc = DateTimeOffset.UtcNow
                });
                await db.SaveChangesAsync();
            }

            Console.WriteLine("✅ Usuario agente.pos creado con contraseña '12345'");
        }

        // Seed usuario cliente.web
        var clienteWebExists = await db.UsuariosApp
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username == "cliente.web");

        if (!clienteWebExists)
        {
            var (hash, salt) = AuthService.CreatePasswordHash("12345");
            
            var clienteUser = new UsuarioAppEntity
            {
                UsuarioGuid = Guid.NewGuid(),
                Username = "cliente.web",
                Correo = "cliente@europcar.dev",
                PasswordHash = hash,
                PasswordSalt = salt,
                RequiereCambioPassword = false,
                EstadoUsuario = "ACT",
                Activo = true,
                CreadoPorUsuario = "SYSTEM",
                FechaRegistroUtc = DateTimeOffset.UtcNow
            };
            db.UsuariosApp.Add(clienteUser);
            await db.SaveChangesAsync();

            var clienteRol = await db.Roles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.NombreRol == "CLIENTE_WEB");
            if (clienteRol != null)
            {
                db.UsuariosRoles.Add(new UsuarioRolEntity
                {
                    IdUsuario = clienteUser.IdUsuario,
                    IdRol = clienteRol.IdRol,
                    EstadoUsuarioRol = "ACT",
                    Activo = true,
                    CreadoPorUsuario = "SYSTEM",
                    FechaRegistroUtc = DateTimeOffset.UtcNow
                });
                await db.SaveChangesAsync();
            }

            Console.WriteLine("✅ Usuario cliente.web creado con contraseña '12345'");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Seed warning: {ex.Message}");
    }
}

// === Pipeline ===
app.UseMiddleware<GlobalExceptionMiddleware>();

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

Console.WriteLine("🚗 Europcar Rental API iniciada");
Console.WriteLine("📖 Swagger: https://localhost:5001/swagger");

app.Run();
