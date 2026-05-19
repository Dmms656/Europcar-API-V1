namespace RedCar.Reservas.DataAccess.Entities;

public sealed class Reserva
{
    public int IdReserva { get; set; }
    public Guid ReservaGuid { get; set; }
    public string CodigoReserva { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public Guid? ClienteGuid { get; set; }
    public int IdVehiculo { get; set; }
    public Guid? VehiculoGuid { get; set; }
    public int IdLocalizacionRecogida { get; set; }
    public Guid? LocalizacionRecogidaGuid { get; set; }
    public int IdLocalizacionDevolucion { get; set; }
    public Guid? LocalizacionDevolucionGuid { get; set; }
    public string CanalReserva { get; set; } = string.Empty;
    public DateTimeOffset FechaHoraRecogida { get; set; }
    public DateTimeOffset FechaHoraDevolucion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ValorImpuestos { get; set; }
    public decimal ValorExtras { get; set; }
    public decimal ValorDepositoGarantia { get; set; }
    public decimal CargoOneWay { get; set; }
    public decimal Total { get; set; }
    public string CodigoConfirmacion { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public bool RequiereHold { get; set; }
    public DateTimeOffset? FechaCancelacionUtc { get; set; }
    public string? MotivoCancelacion { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificadoDesdeIp { get; set; }
    public string OrigenRegistro { get; set; } = string.Empty;
    public long RowVersion { get; set; }

    public ICollection<ReservaConductorLink> Conductores { get; set; } = new List<ReservaConductorLink>();
    public ICollection<ReservaExtraLine> Extras { get; set; } = new List<ReservaExtraLine>();
}

public sealed class ReservaConductorLink
{
    public int IdReservaConductor { get; set; }
    public Guid ReservaConductorGuid { get; set; }
    public int IdReserva { get; set; }
    public Reserva? Reserva { get; set; }
    public int IdConductor { get; set; }
    public Guid? ConductorGuid { get; set; }
    public string TipoConductor { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
    public decimal CargoConductorJoven { get; set; }
    public string EstadoReservaConductor { get; set; } = "ACT";
    public bool EsEliminado { get; set; }
    public DateTimeOffset FechaAsignacionUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificadoDesdeIp { get; set; }
    public string OrigenRegistro { get; set; } = string.Empty;
    public long RowVersion { get; set; }
}

public sealed class ReservaExtraLine
{
    public int IdReservaExtra { get; set; }
    public Guid ReservaExtraGuid { get; set; }
    public int IdReserva { get; set; }
    public Reserva? Reserva { get; set; }
    public int IdExtra { get; set; }
    public Guid? ExtraGuid { get; set; }
    public int Cantidad { get; set; }
    public decimal ValorUnitarioExtra { get; set; }
    public decimal SubtotalExtra { get; set; }
    public string EstadoReservaExtra { get; set; } = "ACT";
    public bool EsEliminado { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificadoDesdeIp { get; set; }
    public string OrigenRegistro { get; set; } = string.Empty;
    public long RowVersion { get; set; }
}

public sealed class Factura
{
    public int IdFactura { get; set; }
    public Guid FacturaGuid { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public Guid? ClienteGuid { get; set; }
    public int? IdReserva { get; set; }
    public Reserva? Reserva { get; set; }
    public int? IdContrato { get; set; }
    public DateTimeOffset FechaEmision { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ValorIva { get; set; }
    public decimal Total { get; set; }
    public string? ObservacionesFactura { get; set; }
    public string? OrigenCanalFactura { get; set; }
    public string EstadoFactura { get; set; } = string.Empty;
    public bool EsEliminado { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = string.Empty;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificacionIp { get; set; }
    public string ServicioOrigen { get; set; } = string.Empty;
    public string? MotivoInhabilitacion { get; set; }
    public long RowVersion { get; set; }
}
