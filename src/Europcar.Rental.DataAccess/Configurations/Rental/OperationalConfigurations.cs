using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Configurations.Rental;

public class ContratoConfiguration : IEntityTypeConfiguration<ContratoEntity>
{
    public void Configure(EntityTypeBuilder<ContratoEntity> builder)
    {
        builder.ToTable("contratos", "rental");
        builder.HasKey(e => e.IdContrato);
        builder.Property(e => e.IdContrato).HasColumnName("id_contrato");
        builder.Property(e => e.ContratoGuid).HasColumnName("contrato_guid");
        builder.Property(e => e.NumeroContrato).HasColumnName("numero_contrato").HasMaxLength(30);
        builder.Property(e => e.IdReserva).HasColumnName("id_reserva");
        builder.Property(e => e.IdCliente).HasColumnName("id_cliente");
        builder.Property(e => e.IdVehiculo).HasColumnName("id_vehiculo");
        builder.Property(e => e.FechaHoraSalida).HasColumnName("fecha_hora_salida");
        builder.Property(e => e.FechaHoraPrevistaDevolucion).HasColumnName("fecha_hora_prevista_devolucion");
        builder.Property(e => e.KilometrajeSalida).HasColumnName("kilometraje_salida");
        builder.Property(e => e.NivelCombustibleSalida).HasColumnName("nivel_combustible_salida").HasPrecision(5, 2);
        builder.Property(e => e.EstadoContrato).HasColumnName("estado_contrato").HasMaxLength(20);
        builder.Property(e => e.PdfUrl).HasColumnName("pdf_url").HasMaxLength(300);
        builder.Property(e => e.ObservacionesContrato).HasColumnName("observaciones_contrato").HasMaxLength(300);
        builder.Property(e => e.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
        // BaseEntity fields that exist in the DB table
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();
        // BaseEntity fields NOT in the DB table — ignore them
        builder.Ignore(e => e.EsEliminado);
        // Navigation
        builder.HasOne(e => e.Reserva).WithMany().HasForeignKey(e => e.IdReserva);
        builder.HasOne(e => e.Cliente).WithMany().HasForeignKey(e => e.IdCliente);
        builder.HasOne(e => e.Vehiculo).WithMany().HasForeignKey(e => e.IdVehiculo);
    }
}

public class CheckInOutConfiguration : IEntityTypeConfiguration<CheckInOutEntity>
{
    public void Configure(EntityTypeBuilder<CheckInOutEntity> builder)
    {
        builder.ToTable("checkin_out", "rental");
        builder.HasKey(e => e.IdCheck);
        builder.Property(e => e.IdCheck).HasColumnName("id_check");
        builder.Property(e => e.CheckGuid).HasColumnName("check_guid");
        builder.Property(e => e.IdContrato).HasColumnName("id_contrato");
        builder.Property(e => e.TipoCheck).HasColumnName("tipo_check").HasMaxLength(10);
        builder.Property(e => e.FechaHoraCheck).HasColumnName("fecha_hora_check");
        builder.Property(e => e.Kilometraje).HasColumnName("kilometraje");
        builder.Property(e => e.NivelCombustible).HasColumnName("nivel_combustible").HasPrecision(5, 2);
        builder.Property(e => e.Limpio).HasColumnName("limpio");
        builder.Property(e => e.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        builder.Property(e => e.CargoCombustible).HasColumnName("cargo_combustible").HasPrecision(10, 2);
        builder.Property(e => e.CargoLimpieza).HasColumnName("cargo_limpieza").HasPrecision(10, 2);
        builder.Property(e => e.CargoKmExtra).HasColumnName("cargo_km_extra").HasPrecision(10, 2);
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();
        builder.HasOne(e => e.Contrato).WithMany(c => c.Checks).HasForeignKey(e => e.IdContrato);
    }
}

public class PagoConfiguration : IEntityTypeConfiguration<PagoEntity>
{
    public void Configure(EntityTypeBuilder<PagoEntity> builder)
    {
        builder.ToTable("pagos", "rental");
        builder.HasKey(e => e.IdPago);
        builder.Property(e => e.IdPago).HasColumnName("id_pago");
        builder.Property(e => e.PagoGuid).HasColumnName("pago_guid");
        builder.Property(e => e.CodigoPago).HasColumnName("codigo_pago").HasMaxLength(30);
        builder.Property(e => e.IdReserva).HasColumnName("id_reserva");
        builder.Property(e => e.IdContrato).HasColumnName("id_contrato");
        builder.Property(e => e.IdCliente).HasColumnName("id_cliente");
        builder.Property(e => e.TipoPago).HasColumnName("tipo_pago").HasMaxLength(20);
        builder.Property(e => e.MetodoPago).HasColumnName("metodo_pago").HasMaxLength(20);
        builder.Property(e => e.EstadoPago).HasColumnName("estado_pago").HasMaxLength(20);
        builder.Property(e => e.ReferenciaExterna).HasColumnName("referencia_externa").HasMaxLength(100);
        builder.Property(e => e.Monto).HasColumnName("monto").HasPrecision(12, 2);
        builder.Property(e => e.Moneda).HasColumnName("moneda").HasMaxLength(3);
        builder.Property(e => e.FechaPagoUtc).HasColumnName("fecha_pago_utc");
        builder.Property(e => e.ObservacionesPago).HasColumnName("observaciones_pago").HasMaxLength(300);
        builder.Property(e => e.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
        // BaseEntity fields that exist in the DB table
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();
        // BaseEntity fields NOT in the DB table — ignore them
        builder.Ignore(e => e.EsEliminado);
        builder.Ignore(e => e.ModificadoDesdeIp);
        // Navigation
        builder.HasOne(e => e.Reserva).WithMany().HasForeignKey(e => e.IdReserva);
        builder.HasOne(e => e.Contrato).WithMany(c => c.Pagos).HasForeignKey(e => e.IdContrato);
        builder.HasOne(e => e.Cliente).WithMany().HasForeignKey(e => e.IdCliente);
    }
}

public class FacturaConfiguration : IEntityTypeConfiguration<FacturaEntity>
{
    public void Configure(EntityTypeBuilder<FacturaEntity> builder)
    {
        builder.ToTable("facturas", "rental");
        builder.HasKey(e => e.IdFactura);
        builder.Property(e => e.IdFactura).HasColumnName("id_factura");
        builder.Property(e => e.FacturaGuid).HasColumnName("factura_guid");
        builder.Property(e => e.NumeroFactura).HasColumnName("numero_factura").HasMaxLength(40);
        builder.Property(e => e.IdCliente).HasColumnName("id_cliente");
        builder.Property(e => e.IdReserva).HasColumnName("id_reserva");
        builder.Property(e => e.IdContrato).HasColumnName("id_contrato");
        builder.Property(e => e.FechaEmision).HasColumnName("fecha_emision");
        builder.Property(e => e.Subtotal).HasColumnName("subtotal").HasPrecision(12, 2);
        builder.Property(e => e.ValorIva).HasColumnName("valor_iva").HasPrecision(12, 2);
        builder.Property(e => e.Total).HasColumnName("total").HasPrecision(12, 2);
        builder.Property(e => e.ObservacionesFactura).HasColumnName("observaciones_factura").HasMaxLength(300);
        builder.Property(e => e.OrigenCanalFactura).HasColumnName("origen_canal_factura").HasMaxLength(50);
        builder.Property(e => e.EstadoFactura).HasColumnName("estado_factura").HasMaxLength(20);
        // BaseEntity fields
        builder.Property(e => e.EsEliminado).HasColumnName("es_eliminado");
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.ModificadoDesdeIp).HasColumnName("modificacion_ip").HasMaxLength(45);
        builder.Property(e => e.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(50);
        builder.Property(e => e.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(250);
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();
        builder.HasOne(e => e.Cliente).WithMany().HasForeignKey(e => e.IdCliente);
        builder.HasOne(e => e.Reserva).WithMany().HasForeignKey(e => e.IdReserva);
        builder.HasOne(e => e.Contrato).WithMany().HasForeignKey(e => e.IdContrato);
        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}

public class MantenimientoConfiguration : IEntityTypeConfiguration<MantenimientoEntity>
{
    public void Configure(EntityTypeBuilder<MantenimientoEntity> builder)
    {
        builder.ToTable("mantenimientos", "rental");
        builder.HasKey(e => e.IdMantenimiento);
        builder.Property(e => e.IdMantenimiento).HasColumnName("id_mantenimiento");
        builder.Property(e => e.MantenimientoGuid).HasColumnName("mantenimiento_guid");
        builder.Property(e => e.CodigoMantenimiento).HasColumnName("codigo_mantenimiento").HasMaxLength(30);
        builder.Property(e => e.IdVehiculo).HasColumnName("id_vehiculo");
        builder.Property(e => e.TipoMantenimiento).HasColumnName("tipo_mantenimiento").HasMaxLength(20);
        builder.Property(e => e.FechaInicioUtc).HasColumnName("fecha_inicio_utc");
        builder.Property(e => e.FechaFinUtc).HasColumnName("fecha_fin_utc");
        builder.Property(e => e.KilometrajeMantenimiento).HasColumnName("kilometraje_mantenimiento");
        builder.Property(e => e.CostoMantenimiento).HasColumnName("costo_mantenimiento").HasPrecision(12, 2);
        builder.Property(e => e.ProveedorTaller).HasColumnName("proveedor_taller").HasMaxLength(120);
        builder.Property(e => e.EstadoMantenimiento).HasColumnName("estado_mantenimiento").HasMaxLength(20);
        builder.Property(e => e.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();
        builder.HasOne(e => e.Vehiculo).WithMany().HasForeignKey(e => e.IdVehiculo);
    }
}

public class LocalizacionExtraStockConfiguration : IEntityTypeConfiguration<LocalizacionExtraStockEntity>
{
    public void Configure(EntityTypeBuilder<LocalizacionExtraStockEntity> builder)
    {
        builder.ToTable("localizacion_extra_stock", "rental");
        builder.HasKey(e => e.IdLocalizacionExtraStock);
        builder.Property(e => e.IdLocalizacionExtraStock).HasColumnName("id_localizacion_extra_stock");
        builder.Property(e => e.StockGuid).HasColumnName("stock_guid");
        builder.Property(e => e.IdLocalizacion).HasColumnName("id_localizacion");
        builder.Property(e => e.IdExtra).HasColumnName("id_extra");
        builder.Property(e => e.StockDisponible).HasColumnName("stock_disponible");
        builder.Property(e => e.StockReservado).HasColumnName("stock_reservado");
        builder.Property(e => e.EstadoStock).HasColumnName("estado_stock").HasMaxLength(3);
        builder.Property(e => e.EsEliminado).HasColumnName("es_eliminado");
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
        builder.Property(e => e.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();
        builder.HasOne(e => e.Localizacion).WithMany().HasForeignKey(e => e.IdLocalizacion);
        builder.HasOne(e => e.Extra).WithMany().HasForeignKey(e => e.IdExtra);
        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}
