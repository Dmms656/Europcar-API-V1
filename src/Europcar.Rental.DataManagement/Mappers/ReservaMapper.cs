using Europcar.Rental.DataAccess.Entities.Rental;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.DataManagement.Mappers;

public static class ReservaMapper
{
    public static ReservaModel ToModel(this ReservaEntity entity) => new()
    {
        IdReserva = entity.IdReserva,
        ReservaGuid = entity.ReservaGuid,
        CodigoReserva = entity.CodigoReserva,
        IdCliente = entity.IdCliente,
        IdVehiculo = entity.IdVehiculo,
        IdLocalizacionRecogida = entity.IdLocalizacionRecogida,
        IdLocalizacionDevolucion = entity.IdLocalizacionDevolucion,
        CanalReserva = entity.CanalReserva,
        FechaHoraRecogida = entity.FechaHoraRecogida,
        FechaHoraDevolucion = entity.FechaHoraDevolucion,
        Subtotal = entity.Subtotal,
        ValorImpuestos = entity.ValorImpuestos,
        ValorExtras = entity.ValorExtras,
        CargoOneWay = entity.CargoOneWay,
        Total = entity.Total,
        CodigoConfirmacion = entity.CodigoConfirmacion,
        EstadoReserva = entity.EstadoReserva,
        NombreCliente = entity.Cliente != null
            ? $"{entity.Cliente.CliNombre1} {entity.Cliente.CliApellido1}"
            : null,
        PlacaVehiculo = entity.Vehiculo?.PlacaVehiculo
    };
}
