using Microsoft.EntityFrameworkCore;

namespace RedCar.Localizaciones.DataAccess.Context;

/// <summary>
/// DbContext de MS.Localizaciones. Apunta al schema <c>localizaciones</c> del proyecto Supabase.
/// Tablas previstas (Fase 2): paises, ciudades, localizaciones.
/// </summary>
public class LocalizacionesDbContext : DbContext
{
    public const string SchemaLocalizaciones = "localizaciones";

    public LocalizacionesDbContext(DbContextOptions<LocalizacionesDbContext> options) : base(options) { }
}
