using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Configurations.Rental;

public class ReservaConfiguration : IEntityTypeConfiguration<ReservaEntity>
{
    public void Configure(EntityTypeBuilder<ReservaEntity> builder)
    {
        builder.ToTable("reservas", "rental");
        builder.HasKey(e => e.IdReserva);

        builder.Property(e => e.IdReserva).HasColumnName("id_reserva");
        builder.Property(e => e.ReservaGuid).HasColumnName("reserva_guid");
        builder.Property(e => e.CodigoReserva).HasColumnName("codigo_reserva").HasMaxLength(20);
        builder.Property(e => e.IdCliente).HasColumnName("id_cliente");
        builder.Property(e => e.IdVehiculo).HasColumnName("id_vehiculo");
        builder.Property(e => e.IdLocalizacionRecogida).HasColumnName("id_localizacion_recogida");
        builder.Property(e => e.IdLocalizacionDevolucion).HasColumnName("id_localizacion_devolucion");
        builder.Property(e => e.CanalReserva).HasColumnName("canal_reserva").HasMaxLength(20);
        builder.Property(e => e.FechaHoraRecogida).HasColumnName("fecha_hora_recogida");
        builder.Property(e => e.FechaHoraDevolucion).HasColumnName("fecha_hora_devolucion");
        builder.Property(e => e.Subtotal).HasColumnName("subtotal").HasPrecision(12, 2);
        builder.Property(e => e.ValorImpuestos).HasColumnName("valor_impuestos").HasPrecision(12, 2);
        builder.Property(e => e.ValorExtras).HasColumnName("valor_extras").HasPrecision(12, 2);
        builder.Property(e => e.ValorDepositoGarantia).HasColumnName("valor_deposito_garantia").HasPrecision(12, 2);
        builder.Property(e => e.CargoOneWay).HasColumnName("cargo_one_way").HasPrecision(12, 2);
        builder.Property(e => e.Total).HasColumnName("total").HasPrecision(12, 2);
        builder.Property(e => e.CodigoConfirmacion).HasColumnName("codigo_confirmacion").HasMaxLength(30);
        builder.Property(e => e.EstadoReserva).HasColumnName("estado_reserva").HasMaxLength(20);
        builder.Property(e => e.RequiereHold).HasColumnName("requiere_hold");
        builder.Property(e => e.FechaCancelacionUtc).HasColumnName("fecha_cancelacion_utc");
        builder.Property(e => e.MotivoCancelacion).HasColumnName("motivo_cancelacion").HasMaxLength(250);
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
        builder.Property(e => e.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();

        // reservas no tiene es_eliminado
        builder.Ignore(e => e.EsEliminado);

        builder.HasOne(e => e.Cliente).WithMany(c => c.Reservas).HasForeignKey(e => e.IdCliente);
        builder.HasOne(e => e.Vehiculo).WithMany(v => v.Reservas).HasForeignKey(e => e.IdVehiculo);
        builder.HasOne(e => e.LocalizacionRecogida).WithMany().HasForeignKey(e => e.IdLocalizacionRecogida);
        builder.HasOne(e => e.LocalizacionDevolucion).WithMany().HasForeignKey(e => e.IdLocalizacionDevolucion);

        builder.HasIndex(e => e.ReservaGuid).IsUnique();
        builder.HasIndex(e => e.CodigoReserva).IsUnique();
        builder.HasIndex(e => e.CodigoConfirmacion).IsUnique();
    }
}
