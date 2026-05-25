using Middleware.RedCar.DataAccess.Clients.Interfaces;

namespace Middleware.RedCar.Api.Compatibility;

/// <summary>Mapeo al contrato del panel administrativo (monolito legacy).</summary>
public static class LegacyAdminDtoMapper
{
    public static object ToMarca(MarcaDto m) => new
    {
        id = m.IdMarca,
        idMarca = m.IdMarca,
        codigo = m.Codigo,
        nombre = m.Nombre,
        estado = m.Estado
    };

    public static object ToCategoria(CategoriaDto c) => new
    {
        id = c.IdCategoria,
        idCategoria = c.IdCategoria,
        codigo = c.Codigo,
        codigoCategoria = c.Codigo,
        nombre = c.Nombre,
        nombreCategoria = c.Nombre,
        descripcion = c.Descripcion,
        estado = c.Estado,
        estadoCategoria = c.Estado
    };

    public static object ToExtra(ExtraDto e) => new
    {
        id = e.IdExtra,
        idExtra = e.IdExtra,
        codigo = e.Codigo,
        codigoExtra = e.Codigo,
        nombre = e.Nombre,
        nombreExtra = e.Nombre,
        descripcion = e.Descripcion,
        descripcionExtra = e.Descripcion,
        valorFijo = e.ValorFijo,
        estado = e.Estado,
        estadoExtra = e.Estado,
        tipoExtra = "SERVICIO",
        requiereStock = false
    };

    public static object ToLocalizacion(LocalizacionDto l) => new
    {
        id = l.IdLocalizacion,
        idLocalizacion = l.IdLocalizacion,
        codigo = l.Codigo,
        codigoLocalizacion = l.Codigo,
        nombre = l.Nombre,
        nombreLocalizacion = l.Nombre,
        direccion = l.Direccion,
        direccionLocalizacion = l.Direccion,
        telefono = l.Telefono,
        telefonoContacto = l.Telefono,
        correo = l.Correo,
        correoContacto = l.Correo,
        horarioAtencion = l.HorarioAtencion,
        zonaHoraria = l.ZonaHoraria,
        estado = l.Estado,
        estadoLocalizacion = l.Estado,
        idCiudad = l.IdCiudad,
        nombreCiudad = l.CiudadNombre
    };

    public static object ToCiudad(CiudadDto c) => new
    {
        id = c.IdCiudad,
        idCiudad = c.IdCiudad,
        idPais = c.IdPais,
        nombre = c.NombreCiudad,
        nombreCiudad = c.NombreCiudad,
        estado = c.EstadoCiudad,
        estadoCiudad = c.EstadoCiudad
    };

    public static object ToPais(PaisDto p) => new
    {
        id = p.Id,
        codigo = p.Codigo,
        nombre = p.Nombre,
        estado = p.Estado
    };

    public static object ToVehiculo(VehiculoAdminDto v) => new
    {
        idVehiculo = v.IdVehiculo,
        vehiculoGuid = v.VehiculoGuid,
        codigoInterno = v.CodigoInterno,
        placa = v.Placa,
        placaVehiculo = v.Placa,
        idMarca = v.IdMarca,
        marca = v.Marca,
        idCategoria = v.IdCategoria,
        categoria = v.Categoria,
        modelo = v.Modelo,
        modeloVehiculo = v.Modelo,
        anioFabricacion = v.AnioFabricacion,
        color = v.Color,
        colorVehiculo = v.Color,
        tipoCombustible = v.TipoCombustible,
        tipoTransmision = v.TipoTransmision,
        capacidadPasajeros = v.CapacidadPasajeros,
        capacidadMaletas = v.CapacidadMaletas,
        numeroPuertas = v.NumeroPuertas,
        precioBaseDia = v.PrecioBaseDia,
        kilometrajeActual = v.KilometrajeActual,
        aireAcondicionado = v.AireAcondicionado,
        estadoOperativo = v.EstadoOperativo,
        observacionesGenerales = v.ObservacionesGenerales,
        imagenReferencialUrl = v.ImagenReferencialUrl,
        idLocalizacion = v.IdLocalizacion,
        localizacion = v.IdLocalizacion.ToString(),
        estadoVehiculo = v.EstadoVehiculo,
        rowVersion = v.RowVersion
    };

    public static object ToCliente(ClienteListItemDto c) => new
    {
        c.IdCliente,
        c.ClienteGuid,
        c.CodigoCliente,
        c.TipoIdentificacion,
        c.NumeroIdentificacion,
        c.NombreCompleto,
        c.Nombre1,
        c.Nombre2,
        c.Apellido1,
        c.Apellido2,
        c.FechaNacimiento,
        telefono = c.Telefono,
        correo = c.Correo,
        c.DireccionPrincipal,
        estadoCliente = c.EstadoCliente,
        c.RowVersion
    };
}
