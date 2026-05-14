using Microsoft.EntityFrameworkCore;

namespace RedCar.Clientes.DataAccess.Context;

/// <summary>
/// DbContext de MS.Clientes. Apunta al schema <c>clientes</c> del proyecto Supabase.
/// Tablas previstas (Fase 2): clientes, conductores.
/// </summary>
public class ClientesDbContext : DbContext
{
    public const string SchemaClientes = "clientes";

    public ClientesDbContext(DbContextOptions<ClientesDbContext> options) : base(options) { }
}
