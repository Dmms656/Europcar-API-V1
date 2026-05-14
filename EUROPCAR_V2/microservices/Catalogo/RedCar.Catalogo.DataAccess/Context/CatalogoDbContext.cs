using Microsoft.EntityFrameworkCore;

namespace RedCar.Catalogo.DataAccess.Context;

/// <summary>
/// DbContext de MS.Catalogo. Apunta al schema <c>catalogo</c> del proyecto Supabase.
/// En la Fase 2 se llenara con las entidades reales (vehiculos, marcas, categorias, extras).
/// </summary>
public class CatalogoDbContext : DbContext
{
    public const string SchemaCatalogo = "catalogo";

    public CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : base(options) { }
}
