using Microsoft.EntityFrameworkCore;
using RedCar.Localizaciones.DataAccess.Entities;

namespace RedCar.Localizaciones.DataAccess.Context;

public class LocalizacionesDbContext : DbContext
{
    public const string SchemaLocalizaciones = "localizaciones";

    public LocalizacionesDbContext(DbContextOptions<LocalizacionesDbContext> options) : base(options) { }

    public DbSet<Ciudad> Ciudades => Set<Ciudad>();
    public DbSet<Localizacion> Localizaciones => Set<Localizacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaLocalizaciones);

        modelBuilder.Entity<Ciudad>(e =>
        {
            e.ToTable("ciudades");
            e.HasKey(x => x.IdCiudad);
            e.Property(x => x.IdCiudad).HasColumnName("id_ciudad");
            e.Property(x => x.CiudadGuid).HasColumnName("ciudad_guid").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.IdPais).HasColumnName("id_pais");
            e.Property(x => x.NombreCiudad).HasColumnName("nombre_ciudad").HasMaxLength(100);
            e.Property(x => x.EstadoCiudad).HasColumnName("estado_ciudad").HasMaxLength(3);
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

        modelBuilder.Entity<Localizacion>(e =>
        {
            e.ToTable("localizaciones");
            e.HasKey(x => x.IdLocalizacion);
            e.Property(x => x.IdLocalizacion).HasColumnName("id_localizacion");
            e.Property(x => x.LocalizacionGuid).HasColumnName("localizacion_guid").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CodigoLocalizacion).HasColumnName("codigo_localizacion").HasMaxLength(20);
            e.Property(x => x.NombreLocalizacion).HasColumnName("nombre_localizacion").HasMaxLength(100);
            e.Property(x => x.IdCiudad).HasColumnName("id_ciudad");
            e.Property(x => x.DireccionLocalizacion).HasColumnName("direccion_localizacion").HasMaxLength(200);
            e.Property(x => x.TelefonoContacto).HasColumnName("telefono_contacto").HasMaxLength(20);
            e.Property(x => x.CorreoContacto).HasColumnName("correo_contacto").HasMaxLength(120);
            e.Property(x => x.HorarioAtencion).HasColumnName("horario_atencion").HasMaxLength(120);
            e.Property(x => x.ZonaHoraria).HasColumnName("zona_horaria").HasMaxLength(50);
            e.Property(x => x.Latitud).HasColumnName("latitud").HasPrecision(9, 6);
            e.Property(x => x.Longitud).HasColumnName("longitud").HasPrecision(9, 6);
            e.Property(x => x.EstadoLocalizacion).HasColumnName("estado_localizacion").HasMaxLength(3);
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

            e.HasOne(x => x.Ciudad)
                .WithMany()
                .HasForeignKey(x => x.IdCiudad);
        });
    }
}
