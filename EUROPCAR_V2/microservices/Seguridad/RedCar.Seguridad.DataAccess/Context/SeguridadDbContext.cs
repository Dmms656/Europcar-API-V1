using Microsoft.EntityFrameworkCore;

namespace RedCar.Seguridad.DataAccess.Context;

/// <summary>
/// DbContext de MS.Seguridad. Apunta a los schemas <c>security</c> y <c>audit</c>
/// del proyecto Supabase. En la Fase 2 se llenara con las entidades reales.
/// </summary>
public class SeguridadDbContext : DbContext
{
    public const string SchemaSecurity = "security";
    public const string SchemaAudit    = "audit";

    public SeguridadDbContext(DbContextOptions<SeguridadDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Default search_path del lado role ms_seguridad ya incluye security y audit,
        // asi que aqui solo declaramos el schema por tabla cuando creemos entidades.
        base.OnModelCreating(modelBuilder);
    }
}
