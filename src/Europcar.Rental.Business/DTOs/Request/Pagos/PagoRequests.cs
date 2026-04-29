namespace Europcar.Rental.Business.DTOs.Request.Pagos;

public class CrearPagoRequest
{
    public int? IdReserva { get; set; }
    public string? CodigoReserva { get; set; }
    public int? IdContrato { get; set; }

    /// <summary>ID numérico del cliente. Mutuamente excluyente con <see cref="CodigoCliente"/>.</summary>
    public int? IdCliente { get; set; }

    /// <summary>Código del cliente (ej: CLI-20240101...). Mutuamente excluyente con <see cref="IdCliente"/>.</summary>
    public string? CodigoCliente { get; set; }

    public string TipoPago { get; set; } = "COBRO";
    public string MetodoPago { get; set; } = "TARJETA";
    public decimal Monto { get; set; }
    public string? ReferenciaExterna { get; set; }
    public string? Observaciones { get; set; }
}

public class ActualizarPagoRequest
{
    public int? IdReserva { get; set; }
    public string? CodigoReserva { get; set; }
    public int? IdContrato { get; set; }

    /// <summary>ID numérico del cliente. Mutuamente excluyente con <see cref="CodigoCliente"/>.</summary>
    public int? IdCliente { get; set; }

    /// <summary>Código del cliente (ej: CLI-20240101...). Mutuamente excluyente con <see cref="IdCliente"/>.</summary>
    public string? CodigoCliente { get; set; }

    public string TipoPago { get; set; } = "COBRO";
    public string MetodoPago { get; set; } = "TARJETA";
    public string EstadoPago { get; set; } = "APROBADO";
    public decimal Monto { get; set; }
    public string? ReferenciaExterna { get; set; }
    public string? Observaciones { get; set; }
}
