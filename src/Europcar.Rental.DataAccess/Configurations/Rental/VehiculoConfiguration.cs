using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Configurations.Rental;

public class VehiculoConfiguration : IEntityTypeConfiguration<VehiculoEntity>
{
    public void Configure(EntityTypeBuilder<VehiculoEntity> builder)
    {
        builder.ToTable("vehiculos", "rental");
        builder.HasKey(e => e.IdVehiculo);

        builder.Property(e => e.IdVehiculo).HasColumnName("id_vehiculo");
        builder.Property(e => e.VehiculoGuid).HasColumnName("vehiculo_guid");
        builder.Property(e => e.CodigoInternoVehiculo).HasColumnName("codigo_interno_vehiculo").HasMaxLength(20);
        builder.Property(e => e.PlacaVehiculo).HasColumnName("placa_vehiculo").HasMaxLength(15);
        builder.Property(e => e.IdMarca).HasColumnName("id_marca");
        builder.Property(e => e.IdCategoria).HasColumnName("id_categoria");
        builder.Property(e => e.ModeloVehiculo).HasColumnName("modelo_vehiculo").HasMaxLength(50);
        builder.Property(e => e.AnioFabricacion).HasColumnName("anio_fabricacion");
        builder.Property(e => e.ColorVehiculo).HasColumnName("color_vehiculo").HasMaxLength(30);
        builder.Property(e => e.TipoCombustible).HasColumnName("tipo_combustible").HasMaxLength(20);
        builder.Property(e => e.TipoTransmision).HasColumnName("tipo_transmision").HasMaxLength(20);
        builder.Property(e => e.CapacidadPasajeros).HasColumnName("capacidad_pasajeros");
        builder.Property(e => e.CapacidadMaletas).HasColumnName("capacidad_maletas");
        builder.Property(e => e.NumeroPuertas).HasColumnName("numero_puertas");
        builder.Property(e => e.LocalizacionActual).HasColumnName("localizacion_actual");
        builder.Property(e => e.PrecioBaseDia).HasColumnName("precio_base_dia").HasPrecision(10, 2);
        builder.Property(e => e.KilometrajeActual).HasColumnName("kilometraje_actual");
        builder.Property(e => e.AireAcondicionado).HasColumnName("aire_acondicionado");
        builder.Property(e => e.EstadoOperativo).HasColumnName("estado_operativo").HasMaxLength(20);
        builder.Property(e => e.ObservacionesGenerales).HasColumnName("observaciones_generales").HasMaxLength(300);
        builder.Property(e => e.ImagenReferencialUrl).HasColumnName("imagen_referencial_url").HasMaxLength(300);
        builder.Property(e => e.EstadoVehiculo).HasColumnName("estado_vehiculo").HasMaxLength(3);
        builder.Property(e => e.EsEliminado).HasColumnName("es_eliminado");
        builder.Property(e => e.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
        builder.Property(e => e.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
        builder.Property(e => e.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(200);
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();

        builder.HasOne(e => e.Marca).WithMany(m => m.Vehiculos).HasForeignKey(e => e.IdMarca);
        builder.HasOne(e => e.Categoria).WithMany(c => c.Vehiculos).HasForeignKey(e => e.IdCategoria);
        builder.HasOne(e => e.Localizacion).WithMany().HasForeignKey(e => e.LocalizacionActual);

        builder.HasIndex(e => e.VehiculoGuid).IsUnique();
        builder.HasIndex(e => e.CodigoInternoVehiculo).IsUnique();
        builder.HasIndex(e => e.PlacaVehiculo).IsUnique();

        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}
