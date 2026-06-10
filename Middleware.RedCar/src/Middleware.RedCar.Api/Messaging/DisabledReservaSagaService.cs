using Middleware.RedCar.DataAccess.GrpcClients.Interfaces;
using Middleware.RedCar.DataManagement.Interfaces;
using Middleware.RedCar.DataManagement.Models.Reservas;

namespace Middleware.RedCar.Api.Messaging;

/// <summary>Saga desactivada (EvB off o RabbitMQ no configurado).</summary>
public sealed class DisabledReservaSagaService : IReservaSagaService
{
    public Task<CrearReservaGrpcResult> CrearReservaViaEventBusAsync(
        CrearReservaGrpcRequest request,
        CancellationToken ct = default)
        => throw new InvalidOperationException("Event Bus deshabilitado (EvB__Enabled=false o RabbitMQ no configurado).");
}
