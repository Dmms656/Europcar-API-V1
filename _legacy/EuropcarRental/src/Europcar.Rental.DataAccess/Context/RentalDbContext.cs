using Microsoft.EntityFrameworkCore;
using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataAccess.Entities.Security;
using Europcar.Rental.DataAccess.Entities.Audit;

namespace Europcar.Rental.DataAccess.Context;

public class RentalDbContext : DbContext
{
    public RentalDbContext(DbContextOptions<RentalDbContext> options) : base(options) { }

    // Rental
    public DbSet<PaisEntity> Paises => Set<PaisEntity>();
    public DbSet<CiudadEntity> Ciudades => Set<CiudadEntity>();
    public DbSet<LocalizacionEntity> Localizaciones => Set<LocalizacionEntity>();
    public DbSet<MarcaVehiculoEntity> MarcaVehiculos => Set<MarcaVehiculoEntity>();
    public DbSet<CategoriaVehiculoEntity> CategoriaVehiculos => Set<CategoriaVehiculoEntity>();
    public DbSet<ExtraEntity> Extras => Set<ExtraEntity>();
    public DbSet<LocalizacionExtraStockEntity> LocalizacionExtraStock => Set<LocalizacionExtraStockEntity>();
    public DbSet<ClienteEntity> Clientes => Set<ClienteEntity>();
    public DbSet<ConductorEntity> Conductores => Set<ConductorEntity>();
    public DbSet<VehiculoEntity> Vehiculos => Set<VehiculoEntity>();
    public DbSet<ReservaEntity> Reservas => Set<ReservaEntity>();
    public DbSet<ReservaExtraEntity> ReservaExtras => Set<ReservaExtraEntity>();
    public DbSet<ReservaConductorEntity> ReservaConductores => Set<ReservaConductorEntity>();
    public DbSet<ContratoEntity> Contratos => Set<ContratoEntity>();
    public DbSet<CheckInOutEntity> CheckInOuts => Set<CheckInOutEntity>();
    public DbSet<PagoEntity> Pagos => Set<PagoEntity>();
    public DbSet<FacturaEntity> Facturas => Set<FacturaEntity>();
    public DbSet<MantenimientoEntity> Mantenimientos => Set<MantenimientoEntity>();

    // Security
    public DbSet<UsuarioAppEntity> UsuariosApp => Set<UsuarioAppEntity>();
    public DbSet<RolEntity> Roles => Set<RolEntity>();
    public DbSet<UsuarioRolEntity> UsuariosRoles => Set<UsuarioRolEntity>();
    public DbSet<PermisoEntity> Permisos => Set<PermisoEntity>();
    public DbSet<RolPermisoEntity> RolesPermisos => Set<RolPermisoEntity>();
    public DbSet<SesionEntity> Sesiones => Set<SesionEntity>();

    // Audit
    public DbSet<AudEventoEntity> AuditoriaEventos => Set<AudEventoEntity>();
    public DbSet<AudIntentoLoginEntity> AuditoriaIntentosLogin => Set<AudIntentoLoginEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RentalDbContext).Assembly);
        
        // AudEvento — configuración inline ya que es simple
        modelBuilder.Entity<AudEventoEntity>(builder =>
        {
            builder.ToTable("aud_eventos", "audit");
            builder.HasKey(e => e.IdAudEvento);
            builder.Property(e => e.IdAudEvento).HasColumnName("id_aud_evento");
            builder.Property(e => e.AudEventoGuid).HasColumnName("aud_evento_guid");
            builder.Property(e => e.EsquemaAfectado).HasColumnName("esquema_afectado").HasMaxLength(50);
            builder.Property(e => e.TablaAfectada).HasColumnName("tabla_afectada").HasMaxLength(100);
            builder.Property(e => e.Operacion).HasColumnName("operacion").HasMaxLength(20);
            builder.Property(e => e.IdRegistroAfectado).HasColumnName("id_registro_afectado").HasMaxLength(100);
            builder.Property(e => e.DatosAnteriores).HasColumnName("datos_anteriores");
            builder.Property(e => e.DatosNuevos).HasColumnName("datos_nuevos");
            builder.Property(e => e.UsuarioApp).HasColumnName("usuario_app").HasMaxLength(100);
            builder.Property(e => e.LoginBd).HasColumnName("login_bd").HasMaxLength(100);
            builder.Property(e => e.IpOrigen).HasColumnName("ip_origen").HasMaxLength(45);
            builder.Property(e => e.OrigenEvento).HasColumnName("origen_evento").HasMaxLength(20);
            builder.Property(e => e.FechaEventoUtc).HasColumnName("fecha_evento_utc");
            builder.Property(e => e.RowVersion).HasColumnName("row_version");
        });

        // AudIntentoLogin
        modelBuilder.Entity<AudIntentoLoginEntity>(builder =>
        {
            builder.ToTable("aud_intentos_login", "audit");
            builder.HasKey(e => e.IdAudLogin);
            builder.Property(e => e.IdAudLogin).HasColumnName("id_aud_login");
            builder.Property(e => e.AudLoginGuid).HasColumnName("aud_login_guid");
            builder.Property(e => e.UsernameIntentado).HasColumnName("username_intentado").HasMaxLength(120);
            builder.Property(e => e.CorreoIntentado).HasColumnName("correo_intentado").HasMaxLength(120);
            builder.Property(e => e.Resultado).HasColumnName("resultado").HasMaxLength(20);
            builder.Property(e => e.Motivo).HasColumnName("motivo").HasMaxLength(200);
            builder.Property(e => e.IpOrigen).HasColumnName("ip_origen").HasMaxLength(45);
            builder.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(300);
            builder.Property(e => e.FechaEventoUtc).HasColumnName("fecha_evento_utc");
            builder.Property(e => e.RowVersion).HasColumnName("row_version");
        });

        // Permiso
        modelBuilder.Entity<PermisoEntity>(builder =>
        {
            builder.ToTable("permisos", "security");
            builder.HasKey(e => e.IdPermiso);
            builder.Property(e => e.IdPermiso).HasColumnName("id_permiso");
            builder.Property(e => e.PermisoGuid).HasColumnName("permiso_guid");
            builder.Property(e => e.CodigoPermiso).HasColumnName("codigo_permiso").HasMaxLength(80);
            builder.Property(e => e.Modulo).HasColumnName("modulo").HasMaxLength(50);
            builder.Property(e => e.Accion).HasColumnName("accion").HasMaxLength(30);
            builder.Property(e => e.DescripcionPermiso).HasColumnName("descripcion_permiso").HasMaxLength(200);
            builder.Property(e => e.EstadoPermiso).HasColumnName("estado_permiso").HasMaxLength(3);
            builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
            builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
            builder.Property(e => e.RowVersion).HasColumnName("row_version");
        });

        // RolPermiso
        modelBuilder.Entity<RolPermisoEntity>(builder =>
        {
            builder.ToTable("roles_permisos", "security");
            builder.HasKey(e => e.IdRolPermiso);
            builder.Property(e => e.IdRolPermiso).HasColumnName("id_rol_permiso");
            builder.Property(e => e.IdRol).HasColumnName("id_rol");
            builder.Property(e => e.IdPermiso).HasColumnName("id_permiso");
            builder.Property(e => e.EstadoRolPermiso).HasColumnName("estado_rol_permiso").HasMaxLength(3);
            builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
            builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
            builder.Property(e => e.RowVersion).HasColumnName("row_version");
            builder.HasOne(e => e.Rol).WithMany().HasForeignKey(e => e.IdRol);
            builder.HasOne(e => e.Permiso).WithMany().HasForeignKey(e => e.IdPermiso);
        });

        // Sesion
        modelBuilder.Entity<SesionEntity>(builder =>
        {
            builder.ToTable("sesiones", "security");
            builder.HasKey(e => e.IdSesion);
            builder.Property(e => e.IdSesion).HasColumnName("id_sesion");
            builder.Property(e => e.SesionGuid).HasColumnName("sesion_guid");
            builder.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            builder.Property(e => e.TokenId).HasColumnName("token_id").HasMaxLength(120);
            builder.Property(e => e.RefreshTokenHash).HasColumnName("refresh_token_hash").HasMaxLength(500);
            builder.Property(e => e.IpOrigen).HasColumnName("ip_origen").HasMaxLength(45);
            builder.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(300);
            builder.Property(e => e.FechaInicioUtc).HasColumnName("fecha_inicio_utc");
            builder.Property(e => e.FechaExpiracionUtc).HasColumnName("fecha_expiracion_utc");
            builder.Property(e => e.FechaCierreUtc).HasColumnName("fecha_cierre_utc");
            builder.Property(e => e.EstadoSesion).HasColumnName("estado_sesion").HasMaxLength(20);
            builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
            builder.Property(e => e.RowVersion).HasColumnName("row_version");
            builder.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.IdUsuario);
        });

        base.OnModelCreating(modelBuilder);
    }
}
