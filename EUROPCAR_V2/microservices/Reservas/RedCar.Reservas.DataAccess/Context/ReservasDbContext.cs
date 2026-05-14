using Microsoft.EntityFrameworkCore;

namespace RedCar.Reservas.DataAccess.Context;

/// <summary>
/// DbContext de MS.Reservas + Facturacion. Apunta al schema <c>reservas</c> del proyecto Supabase.
/// Tablas previstas (Fase 2): reservas, res_x_con, res_x_xtras, facturas, pagos.
/// </summary>
public class ReservasDbContext : DbContext
{
    public const string SchemaReservas = "reservas";

    public ReservasDbContext(DbContextOptions<ReservasDbContext> options) : base(options) { }
}
