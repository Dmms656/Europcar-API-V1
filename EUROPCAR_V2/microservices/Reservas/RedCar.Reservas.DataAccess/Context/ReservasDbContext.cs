using Microsoft.EntityFrameworkCore;
using RedCar.Reservas.DataAccess.Entities;

namespace RedCar.Reservas.DataAccess.Context;

public class ReservasDbContext : DbContext
{
    public const string SchemaReservas = "reservas";

    public ReservasDbContext(DbContextOptions<ReservasDbContext> options) : base(options) { }

    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ReservaConductorLink> ReservaConductores => Set<ReservaConductorLink>();
    public DbSet<ReservaExtraLine> ReservaExtras => Set<ReservaExtraLine>();
    public DbSet<Factura> Facturas => Set<Factura>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaReservas);

        modelBuilder.Entity<Reserva>(e =>
        {
            e.ToTable("reservas");
            e.HasKey(x => x.IdReserva);
            e.Property(x => x.IdReserva).HasColumnName("id_reserva");
            e.Property(x => x.ReservaGuid).HasColumnName("reserva_guid");
            e.Property(x => x.CodigoReserva).HasColumnName("codigo_reserva").HasMaxLength(20);
            e.Property(x => x.IdCliente).HasColumnName("id_cliente");
            e.Property(x => x.IdVehiculo).HasColumnName("id_vehiculo");
            e.Property(x => x.IdLocalizacionRecogida).HasColumnName("id_localizacion_recogida");
            e.Property(x => x.IdLocalizacionDevolucion).HasColumnName("id_localizacion_devolucion");
            e.Property(x => x.CanalReserva).HasColumnName("canal_reserva").HasMaxLength(20);
            e.Property(x => x.FechaHoraRecogida).HasColumnName("fecha_hora_recogida");
            e.Property(x => x.FechaHoraDevolucion).HasColumnName("fecha_hora_devolucion");
            e.Property(x => x.Subtotal).HasColumnName("subtotal").HasPrecision(12, 2);
            e.Property(x => x.ValorImpuestos).HasColumnName("valor_impuestos").HasPrecision(12, 2);
            e.Property(x => x.ValorExtras).HasColumnName("valor_extras").HasPrecision(12, 2);
            e.Property(x => x.ValorDepositoGarantia).HasColumnName("valor_deposito_garantia").HasPrecision(12, 2);
            e.Property(x => x.CargoOneWay).HasColumnName("cargo_one_way").HasPrecision(12, 2);
            e.Property(x => x.Total).HasColumnName("total").HasPrecision(12, 2);
            e.Property(x => x.CodigoConfirmacion).HasColumnName("codigo_confirmacion").HasMaxLength(30);
            e.Property(x => x.EstadoReserva).HasColumnName("estado_reserva").HasMaxLength(20);
            e.Property(x => x.RequiereHold).HasColumnName("requiere_hold");
            e.Property(x => x.FechaCancelacionUtc).HasColumnName("fecha_cancelacion_utc");
            e.Property(x => x.MotivoCancelacion).HasColumnName("motivo_cancelacion").HasMaxLength(250);
            e.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc");

            e.HasMany(x => x.Conductores)
                .WithOne(x => x.Reserva)
                .HasForeignKey(x => x.IdReserva);

            e.HasMany(x => x.Extras)
                .WithOne(x => x.Reserva)
                .HasForeignKey(x => x.IdReserva);
        });

        modelBuilder.Entity<ReservaConductorLink>(e =>
        {
            e.ToTable("reserva_conductores");
            e.HasKey(x => x.IdReservaConductor);
            e.Property(x => x.IdReservaConductor).HasColumnName("id_reserva_conductor");
            e.Property(x => x.IdReserva).HasColumnName("id_reserva");
            e.Property(x => x.IdConductor).HasColumnName("id_conductor");
            e.Property(x => x.TipoConductor).HasColumnName("tipo_conductor").HasMaxLength(20);
            e.Property(x => x.EsPrincipal).HasColumnName("es_principal");
            e.Property(x => x.EstadoReservaConductor).HasColumnName("estado_reserva_conductor").HasMaxLength(3);
            e.Property(x => x.EsEliminado).HasColumnName("es_eliminado");
        });

        modelBuilder.Entity<ReservaExtraLine>(e =>
        {
            e.ToTable("reserva_extras");
            e.HasKey(x => x.IdReservaExtra);
            e.Property(x => x.IdReservaExtra).HasColumnName("id_reserva_extra");
            e.Property(x => x.IdReserva).HasColumnName("id_reserva");
            e.Property(x => x.IdExtra).HasColumnName("id_extra");
            e.Property(x => x.Cantidad).HasColumnName("cantidad");
            e.Property(x => x.ValorUnitarioExtra).HasColumnName("valor_unitario_extra").HasPrecision(10, 2);
            e.Property(x => x.SubtotalExtra).HasColumnName("subtotal_extra").HasPrecision(10, 2);
            e.Property(x => x.EstadoReservaExtra).HasColumnName("estado_reserva_extra").HasMaxLength(3);
            e.Property(x => x.EsEliminado).HasColumnName("es_eliminado");
        });

        modelBuilder.Entity<Factura>(e =>
        {
            e.ToTable("facturas");
            e.HasKey(x => x.IdFactura);
            e.Property(x => x.IdFactura).HasColumnName("id_factura");
            e.Property(x => x.FacturaGuid).HasColumnName("factura_guid");
            e.Property(x => x.NumeroFactura).HasColumnName("numero_factura").HasMaxLength(40);
            e.Property(x => x.IdReserva).HasColumnName("id_reserva");
            e.Property(x => x.FechaEmision).HasColumnName("fecha_emision");
            e.Property(x => x.Subtotal).HasColumnName("subtotal").HasPrecision(12, 2);
            e.Property(x => x.ValorIva).HasColumnName("valor_iva").HasPrecision(12, 2);
            e.Property(x => x.Total).HasColumnName("total").HasPrecision(12, 2);
            e.Property(x => x.EstadoFactura).HasColumnName("estado_factura").HasMaxLength(20);
            e.Property(x => x.EsEliminado).HasColumnName("es_eliminado");

            e.HasOne(x => x.Reserva)
                .WithMany()
                .HasForeignKey(x => x.IdReserva);
        });
    }
}
