using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Europcar.Rental.DataAccess.Entities.Security;

namespace Europcar.Rental.DataAccess.Configurations.Security;

public class UsuarioAppConfiguration : IEntityTypeConfiguration<UsuarioAppEntity>
{
    public void Configure(EntityTypeBuilder<UsuarioAppEntity> builder)
    {
        builder.ToTable("usuarios_app", "security");
        builder.HasKey(e => e.IdUsuario);

        builder.Property(e => e.IdUsuario).HasColumnName("id_usuario");
        builder.Property(e => e.UsuarioGuid).HasColumnName("usuario_guid");
        builder.Property(e => e.Username).HasColumnName("username").HasMaxLength(50);
        builder.Property(e => e.Correo).HasColumnName("correo").HasMaxLength(120);
        builder.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(500);
        builder.Property(e => e.PasswordSalt).HasColumnName("password_salt").HasMaxLength(250);
        builder.Property(e => e.PasswordHint).HasColumnName("password_hint").HasMaxLength(100);
        builder.Property(e => e.RequiereCambioPassword).HasColumnName("requiere_cambio_password");
        builder.Property(e => e.EstadoUsuario).HasColumnName("estado_usuario").HasMaxLength(3);
        builder.Property(e => e.EsEliminado).HasColumnName("es_eliminado");
        builder.Property(e => e.Activo).HasColumnName("activo");
        builder.Property(e => e.IntentosFallidos).HasColumnName("intentos_fallidos");
        builder.Property(e => e.BloqueadoHastaUtc).HasColumnName("bloqueado_hasta_utc");
        builder.Property(e => e.UltimoLoginUtc).HasColumnName("ultimo_login_utc");
        builder.Property(e => e.IdCliente).HasColumnName("id_cliente");
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();

        // usuarios_app no tiene esta columna
        builder.Ignore(e => e.ModificadoDesdeIp);

        builder.HasOne(e => e.Cliente).WithMany().HasForeignKey(e => e.IdCliente);

        builder.HasIndex(e => e.UsuarioGuid).IsUnique();
        builder.HasIndex(e => e.Username).IsUnique();
        builder.HasIndex(e => e.Correo).IsUnique();

        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}

public class RolConfiguration : IEntityTypeConfiguration<RolEntity>
{
    public void Configure(EntityTypeBuilder<RolEntity> builder)
    {
        builder.ToTable("roles", "security");
        builder.HasKey(e => e.IdRol);

        builder.Property(e => e.IdRol).HasColumnName("id_rol");
        builder.Property(e => e.RolGuid).HasColumnName("rol_guid");
        builder.Property(e => e.NombreRol).HasColumnName("nombre_rol").HasMaxLength(50);
        builder.Property(e => e.DescripcionRol).HasColumnName("descripcion_rol").HasMaxLength(200);
        builder.Property(e => e.EsSistema).HasColumnName("es_sistema");
        builder.Property(e => e.EstadoRol).HasColumnName("estado_rol").HasMaxLength(3);
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();

        // roles no tiene estas columnas
        builder.Ignore(e => e.EsEliminado);
        builder.Ignore(e => e.ModificadoDesdeIp);

        builder.HasIndex(e => e.RolGuid).IsUnique();
        builder.HasIndex(e => e.NombreRol).IsUnique();
    }
}

public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRolEntity>
{
    public void Configure(EntityTypeBuilder<UsuarioRolEntity> builder)
    {
        builder.ToTable("usuarios_roles", "security");
        builder.HasKey(e => e.IdUsuarioRol);

        builder.Property(e => e.IdUsuarioRol).HasColumnName("id_usuario_rol");
        builder.Property(e => e.IdUsuario).HasColumnName("id_usuario");
        builder.Property(e => e.IdRol).HasColumnName("id_rol");
        builder.Property(e => e.EstadoUsuarioRol).HasColumnName("estado_usuario_rol").HasMaxLength(3);
        builder.Property(e => e.EsEliminado).HasColumnName("es_eliminado");
        builder.Property(e => e.Activo).HasColumnName("activo");
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();

        // usuarios_roles no tiene esta columna
        builder.Ignore(e => e.ModificadoDesdeIp);

        builder.HasOne(e => e.Usuario).WithMany(u => u.UsuariosRoles).HasForeignKey(e => e.IdUsuario);
        builder.HasOne(e => e.Rol).WithMany(r => r.UsuariosRoles).HasForeignKey(e => e.IdRol);

        builder.HasIndex(e => new { e.IdUsuario, e.IdRol }).IsUnique();

        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}
