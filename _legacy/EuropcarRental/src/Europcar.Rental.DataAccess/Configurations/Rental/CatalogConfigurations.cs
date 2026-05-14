using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Configurations.Rental;

public class PaisConfiguration : IEntityTypeConfiguration<PaisEntity>
{
    public void Configure(EntityTypeBuilder<PaisEntity> builder)
    {
        builder.ToTable("paises", "rental");
        builder.HasKey(e => e.IdPais);
        builder.Property(e => e.IdPais).HasColumnName("id_pais");
        builder.Property(e => e.PaisGuid).HasColumnName("pais_guid");
        builder.Property(e => e.CodigoIso2).HasColumnName("codigo_iso2").HasMaxLength(2);
        builder.Property(e => e.NombrePais).HasColumnName("nombre_pais").HasMaxLength(100);
        builder.Property(e => e.EstadoPais).HasColumnName("estado_pais").HasMaxLength(3);
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
        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}

public class CiudadConfiguration : IEntityTypeConfiguration<CiudadEntity>
{
    public void Configure(EntityTypeBuilder<CiudadEntity> builder)
    {
        builder.ToTable("ciudades", "rental");
        builder.HasKey(e => e.IdCiudad);
        builder.Property(e => e.IdCiudad).HasColumnName("id_ciudad");
        builder.Property(e => e.CiudadGuid).HasColumnName("ciudad_guid");
        builder.Property(e => e.IdPais).HasColumnName("id_pais");
        builder.Property(e => e.NombreCiudad).HasColumnName("nombre_ciudad").HasMaxLength(100);
        builder.Property(e => e.EstadoCiudad).HasColumnName("estado_ciudad").HasMaxLength(3);
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
        builder.HasOne(e => e.Pais).WithMany(p => p.Ciudades).HasForeignKey(e => e.IdPais);
        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}

public class LocalizacionConfiguration : IEntityTypeConfiguration<LocalizacionEntity>
{
    public void Configure(EntityTypeBuilder<LocalizacionEntity> builder)
    {
        builder.ToTable("localizaciones", "rental");
        builder.HasKey(e => e.IdLocalizacion);
        builder.Property(e => e.IdLocalizacion).HasColumnName("id_localizacion");
        builder.Property(e => e.LocalizacionGuid).HasColumnName("localizacion_guid");
        builder.Property(e => e.CodigoLocalizacion).HasColumnName("codigo_localizacion").HasMaxLength(20);
        builder.Property(e => e.NombreLocalizacion).HasColumnName("nombre_localizacion").HasMaxLength(100);
        builder.Property(e => e.IdCiudad).HasColumnName("id_ciudad");
        builder.Property(e => e.DireccionLocalizacion).HasColumnName("direccion_localizacion").HasMaxLength(200);
        builder.Property(e => e.TelefonoContacto).HasColumnName("telefono_contacto").HasMaxLength(20);
        builder.Property(e => e.CorreoContacto).HasColumnName("correo_contacto").HasMaxLength(120);
        builder.Property(e => e.HorarioAtencion).HasColumnName("horario_atencion").HasMaxLength(120);
        builder.Property(e => e.ZonaHoraria).HasColumnName("zona_horaria").HasMaxLength(50);
        builder.Property(e => e.Latitud).HasColumnName("latitud").HasPrecision(9, 6);
        builder.Property(e => e.Longitud).HasColumnName("longitud").HasPrecision(9, 6);
        builder.Property(e => e.EstadoLocalizacion).HasColumnName("estado_localizacion").HasMaxLength(3);
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
        builder.HasOne(e => e.Ciudad).WithMany(c => c.Localizaciones).HasForeignKey(e => e.IdCiudad);
        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}

public class MarcaVehiculoConfiguration : IEntityTypeConfiguration<MarcaVehiculoEntity>
{
    public void Configure(EntityTypeBuilder<MarcaVehiculoEntity> builder)
    {
        builder.ToTable("marca_vehiculos", "rental");
        builder.HasKey(e => e.IdMarca);
        builder.Property(e => e.IdMarca).HasColumnName("id_marca");
        builder.Property(e => e.MarcaGuid).HasColumnName("marca_guid");
        builder.Property(e => e.CodigoMarca).HasColumnName("codigo_marca").HasMaxLength(20);
        builder.Property(e => e.NombreMarca).HasColumnName("nombre_marca").HasMaxLength(100);
        builder.Property(e => e.DescripcionMarca).HasColumnName("descripcion_marca").HasMaxLength(250);
        builder.Property(e => e.EstadoMarca).HasColumnName("estado_marca").HasMaxLength(3);
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
        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}

public class CategoriaVehiculoConfiguration : IEntityTypeConfiguration<CategoriaVehiculoEntity>
{
    public void Configure(EntityTypeBuilder<CategoriaVehiculoEntity> builder)
    {
        builder.ToTable("categoria_vehiculos", "rental");
        builder.HasKey(e => e.IdCategoria);
        builder.Property(e => e.IdCategoria).HasColumnName("id_categoria");
        builder.Property(e => e.CategoriaGuid).HasColumnName("categoria_guid");
        builder.Property(e => e.CodigoCategoria).HasColumnName("codigo_categoria").HasMaxLength(20);
        builder.Property(e => e.NombreCategoria).HasColumnName("nombre_categoria").HasMaxLength(100);
        builder.Property(e => e.DescripcionCategoria).HasColumnName("descripcion_categoria").HasMaxLength(250);
        builder.Property(e => e.KilometrajeIlimitado).HasColumnName("kilometraje_ilimitado");
        builder.Property(e => e.LimiteKmDia).HasColumnName("limite_km_dia");
        builder.Property(e => e.CargoKmExcedente).HasColumnName("cargo_km_excedente").HasPrecision(10, 2);
        builder.Property(e => e.EstadoCategoria).HasColumnName("estado_categoria").HasMaxLength(3);
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
        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}

public class ExtraConfiguration : IEntityTypeConfiguration<ExtraEntity>
{
    public void Configure(EntityTypeBuilder<ExtraEntity> builder)
    {
        builder.ToTable("extras", "rental");
        builder.HasKey(e => e.IdExtra);
        builder.Property(e => e.IdExtra).HasColumnName("id_extra");
        builder.Property(e => e.ExtraGuid).HasColumnName("extra_guid");
        builder.Property(e => e.CodigoExtra).HasColumnName("codigo_extra").HasMaxLength(20);
        builder.Property(e => e.NombreExtra).HasColumnName("nombre_extra").HasMaxLength(100);
        builder.Property(e => e.DescripcionExtra).HasColumnName("descripcion_extra").HasMaxLength(250);
        builder.Property(e => e.TipoExtra).HasColumnName("tipo_extra").HasMaxLength(20);
        builder.Property(e => e.RequiereStock).HasColumnName("requiere_stock");
        builder.Property(e => e.ValorFijo).HasColumnName("valor_fijo").HasPrecision(10, 2);
        builder.Property(e => e.EstadoExtra).HasColumnName("estado_extra").HasMaxLength(3);
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
        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}

public class ConductorConfiguration : IEntityTypeConfiguration<ConductorEntity>
{
    public void Configure(EntityTypeBuilder<ConductorEntity> builder)
    {
        builder.ToTable("conductores", "rental");
        builder.HasKey(e => e.IdConductor);
        builder.Property(e => e.IdConductor).HasColumnName("id_conductor");
        builder.Property(e => e.ConductorGuid).HasColumnName("conductor_guid");
        builder.Property(e => e.CodigoConductor).HasColumnName("codigo_conductor").HasMaxLength(20);
        builder.Property(e => e.IdCliente).HasColumnName("id_cliente");
        builder.Property(e => e.TipoIdentificacion).HasColumnName("tipo_identificacion").HasMaxLength(10);
        builder.Property(e => e.NumeroIdentificacion).HasColumnName("numero_identificacion").HasMaxLength(20);
        builder.Property(e => e.ConNombre1).HasColumnName("con_nombre1").HasMaxLength(80);
        builder.Property(e => e.ConNombre2).HasColumnName("con_nombre2").HasMaxLength(80);
        builder.Property(e => e.ConApellido1).HasColumnName("con_apellido1").HasMaxLength(80);
        builder.Property(e => e.ConApellido2).HasColumnName("con_apellido2").HasMaxLength(80);
        builder.Property(e => e.NumeroLicencia).HasColumnName("numero_licencia").HasMaxLength(30);
        builder.Property(e => e.FechaVencimientoLicencia).HasColumnName("fecha_vencimiento_licencia");
        builder.Property(e => e.EdadConductor).HasColumnName("edad_conductor");
        builder.Property(e => e.ConTelefono).HasColumnName("con_telefono").HasMaxLength(20);
        builder.Property(e => e.ConCorreo).HasColumnName("con_correo").HasMaxLength(120);
        builder.Property(e => e.EsConductorJoven).HasColumnName("es_conductor_joven")
            .ValueGeneratedOnAddOrUpdate();
        builder.Property(e => e.EstadoConductor).HasColumnName("estado_conductor").HasMaxLength(3);
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
        builder.HasOne(e => e.Cliente).WithMany(c => c.Conductores).HasForeignKey(e => e.IdCliente);
        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}

public class ReservaExtraConfiguration : IEntityTypeConfiguration<ReservaExtraEntity>
{
    public void Configure(EntityTypeBuilder<ReservaExtraEntity> builder)
    {
        builder.ToTable("reserva_extras", "rental");
        builder.HasKey(e => e.IdReservaExtra);
        builder.Property(e => e.IdReservaExtra).HasColumnName("id_reserva_extra");
        builder.Property(e => e.ReservaExtraGuid).HasColumnName("reserva_extra_guid");
        builder.Property(e => e.IdReserva).HasColumnName("id_reserva");
        builder.Property(e => e.IdExtra).HasColumnName("id_extra");
        builder.Property(e => e.Cantidad).HasColumnName("cantidad");
        builder.Property(e => e.ValorUnitarioExtra).HasColumnName("valor_unitario_extra").HasPrecision(10, 2);
        builder.Property(e => e.SubtotalExtra).HasColumnName("subtotal_extra").HasPrecision(10, 2);
        builder.Property(e => e.EstadoReservaExtra).HasColumnName("estado_reserva_extra").HasMaxLength(3);
        builder.Property(e => e.EsEliminado).HasColumnName("es_eliminado");
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
        builder.Property(e => e.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();
        builder.HasOne(e => e.Reserva).WithMany(r => r.Extras).HasForeignKey(e => e.IdReserva);
        builder.HasOne(e => e.Extra).WithMany().HasForeignKey(e => e.IdExtra);
        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}

public class ReservaConductorConfiguration : IEntityTypeConfiguration<ReservaConductorEntity>
{
    public void Configure(EntityTypeBuilder<ReservaConductorEntity> builder)
    {
        builder.ToTable("reserva_conductores", "rental");
        builder.HasKey(e => e.IdReservaConductor);
        builder.Property(e => e.IdReservaConductor).HasColumnName("id_reserva_conductor");
        builder.Property(e => e.ReservaConductorGuid).HasColumnName("reserva_conductor_guid");
        builder.Property(e => e.IdReserva).HasColumnName("id_reserva");
        builder.Property(e => e.IdConductor).HasColumnName("id_conductor");
        builder.Property(e => e.TipoConductor).HasColumnName("tipo_conductor").HasMaxLength(20);
        builder.Property(e => e.EsPrincipal).HasColumnName("es_principal");
        builder.Property(e => e.CargoConductorJoven).HasColumnName("cargo_conductor_joven").HasPrecision(10, 2);
        builder.Property(e => e.EstadoReservaConductor).HasColumnName("estado_reserva_conductor").HasMaxLength(3);
        builder.Property(e => e.EsEliminado).HasColumnName("es_eliminado");
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_asignacion_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.ModificadoDesdeIp).HasColumnName("modificado_desde_ip").HasMaxLength(45);
        builder.Property(e => e.OrigenRegistro).HasColumnName("origen_registro").HasMaxLength(20);
        builder.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken();
        builder.HasOne(e => e.Reserva).WithMany(r => r.Conductores).HasForeignKey(e => e.IdReserva);
        builder.HasOne(e => e.Conductor).WithMany().HasForeignKey(e => e.IdConductor);
        builder.HasQueryFilter(e => !e.EsEliminado);
    }
}
