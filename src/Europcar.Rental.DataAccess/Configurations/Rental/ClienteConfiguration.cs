using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Configurations.Rental;

public class ClienteConfiguration : IEntityTypeConfiguration<ClienteEntity>
{
    public void Configure(EntityTypeBuilder<ClienteEntity> builder)
    {
        builder.ToTable("clientes", "rental");
        builder.HasKey(e => e.IdCliente);

        builder.Property(e => e.IdCliente).HasColumnName("id_cliente");
        builder.Property(e => e.ClienteGuid).HasColumnName("cliente_guid");
        builder.Property(e => e.CodigoCliente).HasColumnName("codigo_cliente").HasMaxLength(20);
        builder.Property(e => e.TipoIdentificacion).HasColumnName("tipo_identificacion").HasMaxLength(10);
        builder.Property(e => e.NumeroIdentificacion).HasColumnName("numero_identificacion").HasMaxLength(20);
        builder.Property(e => e.CliNombre1).HasColumnName("cli_nombre1").HasMaxLength(80);
        builder.Property(e => e.CliNombre2).HasColumnName("cli_nombre2").HasMaxLength(80);
        builder.Property(e => e.CliApellido1).HasColumnName("cli_apellido1").HasMaxLength(80);
        builder.Property(e => e.CliApellido2).HasColumnName("cli_apellido2").HasMaxLength(80);
        builder.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
        builder.Property(e => e.CliTelefono).HasColumnName("cli_telefono").HasMaxLength(20);
        builder.Property(e => e.CliCorreo).HasColumnName("cli_correo").HasMaxLength(120);
        builder.Property(e => e.DireccionPrincipal).HasColumnName("direccion_principal").HasMaxLength(200);
        builder.Property(e => e.EstadoCliente).HasColumnName("estado_cliente").HasMaxLength(3);
        builder.Property(e => e.EsEliminado).HasColumnName("es_eliminado");
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
        builder.Property(e => e.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
        builder.Property(e => e.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(200);
        builder.Property(e => e.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();

        builder.HasIndex(e => e.ClienteGuid).IsUnique();
        builder.HasIndex(e => e.CodigoCliente).IsUnique();
        builder.HasIndex(e => e.NumeroIdentificacion).IsUnique();
        builder.HasIndex(e => e.CliCorreo).IsUnique();

        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}
