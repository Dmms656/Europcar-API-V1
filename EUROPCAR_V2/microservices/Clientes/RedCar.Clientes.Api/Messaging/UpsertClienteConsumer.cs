using MassTransit;
using RedCar.Clientes.Api.Services;
using RedCar.Shared.Events;
using RedCar.Shared.Events.Reservas;

namespace RedCar.Clientes.Api.Messaging;

public sealed class UpsertClienteConsumer : IConsumer<UpsertClienteCommand>
{
    private readonly ClientesWriteService _write;
    private readonly IPublishEndpoint _publish;

    public UpsertClienteConsumer(ClientesWriteService write, IPublishEndpoint publish)
    {
        _write = write;
        _publish = publish;
    }

    public async Task Consume(ConsumeContext<UpsertClienteCommand> context)
    {
        var cmd = context.Message;
        var ct = context.CancellationToken;

        var (cliente, conductores) = await _write.UpsertBookingAsync(cmd.Cliente, cmd.Conductores, ct);

        var payload = new ClienteActualizadoPayload(
            cmd.CorrelationId,
            cliente.IdCliente,
            cliente.ClienteGuid,
            cliente.Created,
            cmd.Conductores.Select(src =>
            {
                var c = conductores.First(x => x.NumeroIdentificacion == src.NumeroIdentificacion);
                return new ConductorRegistradoPayload(
                    c.IdConductor, src.Nombres, src.Apellidos, src.TipoIdentificacion,
                    src.NumeroIdentificacion, src.FechaVencimientoLicencia, src.EdadConductor,
                    src.Correo, src.Telefono, src.EsPrincipal);
            }).ToList());

        var envelope = EventEnvelope<ClienteActualizadoPayload>.Create(
            RoutingKeys.ClienteActualizado,
            cmd.CorrelationId,
            "RedCar.Clientes",
            payload);

        await _publish.Publish(envelope, ct);
    }
}
