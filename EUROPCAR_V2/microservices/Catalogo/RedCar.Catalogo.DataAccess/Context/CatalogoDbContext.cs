using Microsoft.EntityFrameworkCore;
using RedCar.Catalogo.DataAccess.Entities;

namespace RedCar.Catalogo.DataAccess.Context;

public class CatalogoDbContext : DbContext
{
    public const string SchemaCatalogo = "catalogo";

    public CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : base(options) { }

    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<MarcaVehiculo> Marcas => Set<MarcaVehiculo>();
    public DbSet<CategoriaVehiculo> Categorias => Set<CategoriaVehiculo>();
    public DbSet<Extra> Extras => Set<Extra>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaCatalogo);

        modelBuilder.Entity<MarcaVehiculo>(e =>
        {
            e.ToTable("marca_vehiculos");
            e.HasKey(x => x.IdMarca);
            e.Property(x => x.IdMarca).HasColumnName("id_marca");
            e.Property(x => x.MarcaGuid).HasColumnName("marca_guid").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CodigoMarca).HasColumnName("codigo_marca").HasMaxLength(20);
            e.Property(x => x.NombreMarca).HasColumnName("nombre_marca").HasMaxLength(100);
            e.Property(x => x.DescripcionMarca).HasColumnName("descripcion_marca").HasMaxLength(250);
            e.Property(x => x.EstadoMarca).HasColumnName("estado_marca").HasMaxLength(3);
            e.Property(x => x.EsEliminado).HasColumnName("es_eliminado");
            e.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").HasDefaultValueSql("CURRENT_TIMESTAMP(0)");
            e.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
            e.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
            e.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
            e.Property(x => x.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
            e.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
            e.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(200);
            e.Property(x => x.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
            e.Property(x => x.RowVersion).HasColumnName("row_version").HasDefaultValue(1L);
        });

        modelBuilder.Entity<CategoriaVehiculo>(e =>
        {
            e.ToTable("categoria_vehiculos");
            e.HasKey(x => x.IdCategoria);
            e.Property(x => x.IdCategoria).HasColumnName("id_categoria");
            e.Property(x => x.CategoriaGuid).HasColumnName("categoria_guid").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CodigoCategoria).HasColumnName("codigo_categoria").HasMaxLength(20);
            e.Property(x => x.NombreCategoria).HasColumnName("nombre_categoria").HasMaxLength(100);
            e.Property(x => x.DescripcionCategoria).HasColumnName("descripcion_categoria").HasMaxLength(250);
            e.Property(x => x.KilometrajeIlimitado).HasColumnName("kilometraje_ilimitado").HasDefaultValue(true);
            e.Property(x => x.LimiteKmDia).HasColumnName("limite_km_dia");
            e.Property(x => x.CargoKmExcedente).HasColumnName("cargo_km_excedente").HasPrecision(10, 2);
            e.Property(x => x.EstadoCategoria).HasColumnName("estado_categoria").HasMaxLength(3);
            e.Property(x => x.EsEliminado).HasColumnName("es_eliminado");
            e.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").HasDefaultValueSql("CURRENT_TIMESTAMP(0)");
            e.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
            e.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
            e.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
            e.Property(x => x.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
            e.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
            e.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(200);
            e.Property(x => x.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
            e.Property(x => x.RowVersion).HasColumnName("row_version").HasDefaultValue(1L);
        });

        modelBuilder.Entity<Extra>(e =>
        {
            e.ToTable("extras");
            e.HasKey(x => x.IdExtra);
            e.Property(x => x.IdExtra).HasColumnName("id_extra");
            e.Property(x => x.ExtraGuid).HasColumnName("extra_guid").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CodigoExtra).HasColumnName("codigo_extra").HasMaxLength(20);
            e.Property(x => x.NombreExtra).HasColumnName("nombre_extra").HasMaxLength(100);
            e.Property(x => x.DescripcionExtra).HasColumnName("descripcion_extra").HasMaxLength(250);
            e.Property(x => x.TipoExtra).HasColumnName("tipo_extra").HasMaxLength(20).HasDefaultValue("SERVICIO");
            e.Property(x => x.RequiereStock).HasColumnName("requiere_stock");
            e.Property(x => x.ValorFijo).HasColumnName("valor_fijo").HasPrecision(10, 2);
            e.Property(x => x.EstadoExtra).HasColumnName("estado_extra").HasMaxLength(3);
            e.Property(x => x.EsEliminado).HasColumnName("es_eliminado");
            e.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").HasDefaultValueSql("CURRENT_TIMESTAMP(0)");
            e.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
            e.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
            e.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
            e.Property(x => x.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
            e.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
            e.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(200);
            e.Property(x => x.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
            e.Property(x => x.RowVersion).HasColumnName("row_version").HasDefaultValue(1L);
        });

        modelBuilder.Entity<Vehiculo>(e =>
        {
            e.ToTable("vehiculos");
            e.HasKey(x => x.IdVehiculo);
            e.Property(x => x.IdVehiculo).HasColumnName("id_vehiculo");
            e.Property(x => x.VehiculoGuid).HasColumnName("vehiculo_guid").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CodigoInternoVehiculo).HasColumnName("codigo_interno_vehiculo").HasMaxLength(20);
            e.Property(x => x.PlacaVehiculo).HasColumnName("placa_vehiculo").HasMaxLength(15);
            e.Property(x => x.IdMarca).HasColumnName("id_marca");
            e.Property(x => x.IdCategoria).HasColumnName("id_categoria");
            e.Property(x => x.ModeloVehiculo).HasColumnName("modelo_vehiculo").HasMaxLength(50);
            e.Property(x => x.AnioFabricacion).HasColumnName("anio_fabricacion");
            e.Property(x => x.ColorVehiculo).HasColumnName("color_vehiculo").HasMaxLength(30);
            e.Property(x => x.TipoCombustible).HasColumnName("tipo_combustible").HasMaxLength(20);
            e.Property(x => x.TipoTransmision).HasColumnName("tipo_transmision").HasMaxLength(20);
            e.Property(x => x.CapacidadPasajeros).HasColumnName("capacidad_pasajeros");
            e.Property(x => x.CapacidadMaletas).HasColumnName("capacidad_maletas");
            e.Property(x => x.NumeroPuertas).HasColumnName("numero_puertas");
            e.Property(x => x.LocalizacionActual).HasColumnName("localizacion_actual");
            e.Property(x => x.LocalizacionGuid).HasColumnName("localizacion_guid");
            e.Property(x => x.PrecioBaseDia).HasColumnName("precio_base_dia").HasPrecision(10, 2);
            e.Property(x => x.KilometrajeActual).HasColumnName("kilometraje_actual");
            e.Property(x => x.AireAcondicionado).HasColumnName("aire_acondicionado");
            e.Property(x => x.EstadoOperativo).HasColumnName("estado_operativo").HasMaxLength(20);
            e.Property(x => x.ObservacionesGenerales).HasColumnName("observaciones_generales").HasMaxLength(300);
            e.Property(x => x.ImagenReferencialUrl).HasColumnName("imagen_referencial_url").HasMaxLength(300);
            e.Property(x => x.EstadoVehiculo).HasColumnName("estado_vehiculo").HasMaxLength(3);
            e.Property(x => x.EsEliminado).HasColumnName("es_eliminado");
            e.Property(x => x.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
            e.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").HasDefaultValueSql("CURRENT_TIMESTAMP(0)");
            e.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
            e.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
            e.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
            e.Property(x => x.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
            e.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
            e.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(200);
            e.Property(x => x.RowVersion).HasColumnName("row_version").HasDefaultValue(1L);

            e.HasOne(x => x.Marca)
                .WithMany()
                .HasForeignKey(x => x.IdMarca);

            e.HasOne(x => x.Categoria)
                .WithMany()
                .HasForeignKey(x => x.IdCategoria);
        });
    }
}
