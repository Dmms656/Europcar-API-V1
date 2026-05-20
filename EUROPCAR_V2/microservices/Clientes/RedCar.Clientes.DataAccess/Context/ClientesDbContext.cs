using Microsoft.EntityFrameworkCore;
using RedCar.Clientes.DataAccess.Entities;

namespace RedCar.Clientes.DataAccess.Context;

public class ClientesDbContext : DbContext
{
    public const string SchemaClientes = "clientes";

    public ClientesDbContext(DbContextOptions<ClientesDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Conductor> Conductores => Set<Conductor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaClientes);

        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("clientes");
            e.HasKey(x => x.IdCliente);
            e.Property(x => x.IdCliente).HasColumnName("id_cliente").ValueGeneratedOnAdd();
            e.Property(x => x.ClienteGuid).HasColumnName("cliente_guid").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CodigoCliente).HasColumnName("codigo_cliente").HasMaxLength(20);
            e.Property(x => x.TipoIdentificacion).HasColumnName("tipo_identificacion").HasMaxLength(10);
            e.Property(x => x.NumeroIdentificacion).HasColumnName("numero_identificacion").HasMaxLength(20);
            e.Property(x => x.CliNombre1).HasColumnName("cli_nombre1").HasMaxLength(80);
            e.Property(x => x.CliNombre2).HasColumnName("cli_nombre2").HasMaxLength(80);
            e.Property(x => x.CliApellido1).HasColumnName("cli_apellido1").HasMaxLength(80);
            e.Property(x => x.CliApellido2).HasColumnName("cli_apellido2").HasMaxLength(80);
            e.Property(x => x.FechaNacimiento).HasColumnName("fecha_nacimiento");
            e.Property(x => x.CliTelefono).HasColumnName("cli_telefono").HasMaxLength(20);
            e.Property(x => x.CliCorreo).HasColumnName("cli_correo").HasMaxLength(120);
            e.Property(x => x.DireccionPrincipal).HasColumnName("direccion_principal").HasMaxLength(200);
            e.Property(x => x.EstadoCliente).HasColumnName("estado_cliente").HasMaxLength(3);
            e.Property(x => x.EsEliminado).HasColumnName("es_eliminado");
            e.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
            e.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
            e.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
            e.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
            e.Property(x => x.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
            e.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
            e.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(200);
            e.Property(x => x.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
            e.Property(x => x.RowVersion).HasColumnName("row_version").HasDefaultValue(1L);
        });

        modelBuilder.Entity<Conductor>(e =>
        {
            e.ToTable("conductores");
            e.HasKey(x => x.IdConductor);
            e.Property(x => x.IdConductor).HasColumnName("id_conductor").ValueGeneratedOnAdd();
            e.Property(x => x.ConductorGuid).HasColumnName("conductor_guid").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CodigoConductor).HasColumnName("codigo_conductor").HasMaxLength(20);
            e.Property(x => x.IdCliente).HasColumnName("id_cliente");
            e.Property(x => x.TipoIdentificacion).HasColumnName("tipo_identificacion").HasMaxLength(10);
            e.Property(x => x.NumeroIdentificacion).HasColumnName("numero_identificacion").HasMaxLength(20);
            e.Property(x => x.ConNombre1).HasColumnName("con_nombre1").HasMaxLength(80);
            e.Property(x => x.ConNombre2).HasColumnName("con_nombre2").HasMaxLength(80);
            e.Property(x => x.ConApellido1).HasColumnName("con_apellido1").HasMaxLength(80);
            e.Property(x => x.ConApellido2).HasColumnName("con_apellido2").HasMaxLength(80);
            e.Property(x => x.NumeroLicencia).HasColumnName("numero_licencia").HasMaxLength(30);
            e.Property(x => x.FechaVencimientoLicencia).HasColumnName("fecha_vencimiento_licencia");
            e.Property(x => x.EdadConductor).HasColumnName("edad_conductor");
            e.Property(x => x.ConTelefono).HasColumnName("con_telefono").HasMaxLength(20);
            e.Property(x => x.ConCorreo).HasColumnName("con_correo").HasMaxLength(120);
            e.Property(x => x.EsConductorJoven)
                .HasColumnName("es_conductor_joven")
                .HasComputedColumnSql("edad_conductor BETWEEN 21 AND 24", stored: true)
                .ValueGeneratedOnAddOrUpdate();
            e.Property(x => x.EstadoConductor).HasColumnName("estado_conductor").HasMaxLength(3);
            e.Property(x => x.EsEliminado).HasColumnName("es_eliminado");
            e.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
            e.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
            e.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
            e.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
            e.Property(x => x.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
            e.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
            e.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(200);
            e.Property(x => x.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
            e.Property(x => x.RowVersion).HasColumnName("row_version").HasDefaultValue(1L);
        });
    }
}
