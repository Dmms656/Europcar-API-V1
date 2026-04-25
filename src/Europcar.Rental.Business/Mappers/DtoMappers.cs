using Europcar.Rental.Business.DTOs.Request.Clientes;
using Europcar.Rental.Business.DTOs.Response.Clientes;
using Europcar.Rental.Business.DTOs.Response.Vehiculos;
using Europcar.Rental.Business.DTOs.Response.Reservas;
using Europcar.Rental.Business.DTOs.Response.Contratos;
using Europcar.Rental.Business.DTOs.Response.Pagos;
using Europcar.Rental.Business.DTOs.Response.Mantenimientos;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.Business.Mappers;

/// <summary>
/// Extensiones de mapeo Model ↔ DTO para la capa Business.
/// </summary>
public static class ClienteDtoMapper
{
    public static ClienteResponse ToResponse(this ClienteModel model) => new()
    {
        IdCliente = model.IdCliente,
        ClienteGuid = model.ClienteGuid,
        CodigoCliente = model.CodigoCliente,
        TipoIdentificacion = model.TipoIdentificacion,
        NumeroIdentificacion = model.NumeroIdentificacion,
        NombreCompleto = $"{model.Nombre1} {model.Nombre2 ?? ""} {model.Apellido1} {model.Apellido2 ?? ""}".Trim(),
        Nombre1 = model.Nombre1,
        Nombre2 = model.Nombre2,
        Apellido1 = model.Apellido1,
        Apellido2 = model.Apellido2,
        FechaNacimiento = model.FechaNacimiento,
        Telefono = model.Telefono,
        Correo = model.Correo,
        DireccionPrincipal = model.DireccionPrincipal,
        EstadoCliente = model.EstadoCliente,
        RowVersion = model.RowVersion
    };

    public static ClienteModel ToModel(this CrearClienteRequest request, string codigoCliente) => new()
    {
        CodigoCliente = codigoCliente,
        TipoIdentificacion = request.TipoIdentificacion.ToUpper(),
        NumeroIdentificacion = request.NumeroIdentificacion,
        Nombre1 = request.Nombre1,
        Nombre2 = request.Nombre2,
        Apellido1 = request.Apellido1,
        Apellido2 = request.Apellido2,
        FechaNacimiento = request.FechaNacimiento,
        Telefono = request.Telefono,
        Correo = request.Correo,
        DireccionPrincipal = request.DireccionPrincipal
    };

    public static ClienteModel ToModel(this ActualizarClienteRequest request, int id, string codigoExistente) => new()
    {
        IdCliente = id,
        CodigoCliente = codigoExistente,
        TipoIdentificacion = request.TipoIdentificacion.ToUpper(),
        NumeroIdentificacion = request.NumeroIdentificacion,
        Nombre1 = request.Nombre1,
        Nombre2 = request.Nombre2,
        Apellido1 = request.Apellido1,
        Apellido2 = request.Apellido2,
        FechaNacimiento = request.FechaNacimiento,
        Telefono = request.Telefono,
        Correo = request.Correo,
        DireccionPrincipal = request.DireccionPrincipal,
        RowVersion = request.RowVersion
    };
}

public static class VehiculoDtoMapper
{
    public static VehiculoDisponibleResponse ToResponse(this VehiculoModel model) => new()
    {
        IdVehiculo = model.IdVehiculo,
        VehiculoGuid = model.VehiculoGuid,
        CodigoInterno = model.CodigoInterno,
        Placa = model.Placa,
        Marca = model.Marca,
        Categoria = model.Categoria,
        Modelo = model.Modelo,
        AnioFabricacion = model.AnioFabricacion,
        Color = model.Color,
        TipoCombustible = model.TipoCombustible,
        TipoTransmision = model.TipoTransmision,
        CapacidadPasajeros = model.CapacidadPasajeros,
        CapacidadMaletas = model.CapacidadMaletas,
        PrecioBaseDia = model.PrecioBaseDia,
        AireAcondicionado = model.AireAcondicionado,
        ImagenUrl = model.ImagenUrl,
        IdLocalizacion = model.IdLocalizacion,
        Localizacion = model.NombreLocalizacion
    };
}

public static class ReservaDtoMapper
{
    public static ReservaResponse ToResponse(this ReservaModel model) => new()
    {
        IdReserva = model.IdReserva,
        ReservaGuid = model.ReservaGuid,
        CodigoReserva = model.CodigoReserva,
        CodigoConfirmacion = model.CodigoConfirmacion,
        EstadoReserva = model.EstadoReserva,
        FechaHoraRecogida = model.FechaHoraRecogida,
        FechaHoraDevolucion = model.FechaHoraDevolucion,
        Subtotal = model.Subtotal,
        ValorImpuestos = model.ValorImpuestos,
        Total = model.Total,
        NombreCliente = model.NombreCliente,
        PlacaVehiculo = model.PlacaVehiculo
    };
}

public static class ContratoDtoMapper
{
    public static ContratoResponse ToResponse(this ContratoModel model) => new()
    {
        IdContrato = model.IdContrato,
        ContratoGuid = model.ContratoGuid,
        NumeroContrato = model.NumeroContrato,
        EstadoContrato = model.EstadoContrato,
        FechaHoraSalida = model.FechaHoraSalida,
        FechaHoraPrevistaDevolucion = model.FechaHoraPrevistaDevolucion,
        KilometrajeSalida = model.KilometrajeSalida,
        NivelCombustibleSalida = model.NivelCombustibleSalida,
        NombreCliente = model.NombreCliente,
        PlacaVehiculo = model.PlacaVehiculo,
        CodigoReserva = model.CodigoReserva,
        ObservacionesContrato = model.ObservacionesContrato
    };
}

public static class PagoDtoMapper
{
    public static PagoResponse ToResponse(this PagoModel model) => new()
    {
        IdPago = model.IdPago,
        PagoGuid = model.PagoGuid,
        CodigoPago = model.CodigoPago,
        TipoPago = model.TipoPago,
        MetodoPago = model.MetodoPago,
        EstadoPago = model.EstadoPago,
        Monto = model.Monto,
        Moneda = model.Moneda,
        FechaPagoUtc = model.FechaPagoUtc,
        ReferenciaExterna = model.ReferenciaExterna,
        NombreCliente = model.NombreCliente,
        CodigoReserva = model.CodigoReserva,
        ObservacionesPago = model.ObservacionesPago
    };
}

public static class MantenimientoDtoMapper
{
    public static MantenimientoResponse ToResponse(this MantenimientoModel model) => new()
    {
        IdMantenimiento = model.IdMantenimiento,
        MantenimientoGuid = model.MantenimientoGuid,
        CodigoMantenimiento = model.CodigoMantenimiento,
        TipoMantenimiento = model.TipoMantenimiento,
        FechaInicioUtc = model.FechaInicioUtc,
        FechaFinUtc = model.FechaFinUtc,
        KilometrajeMantenimiento = model.KilometrajeMantenimiento,
        CostoMantenimiento = model.CostoMantenimiento,
        ProveedorTaller = model.ProveedorTaller,
        EstadoMantenimiento = model.EstadoMantenimiento,
        Observaciones = model.Observaciones,
        PlacaVehiculo = model.PlacaVehiculo
    };
}
