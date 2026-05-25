namespace RedCar.Clientes.Api.Contracts;

public sealed class ClienteUpsertRequest
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}

public sealed class ClienteUpsertResult
{
    public int IdCliente { get; set; }
    public Guid ClienteGuid { get; set; }
    public bool Created { get; set; }
}

public sealed class ConductorUpsertRequest
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public DateOnly FechaVencimientoLicencia { get; set; }
    public int EdadConductor { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
}

public sealed class ConductorUpsertResult
{
    public int IdConductor { get; set; }
    public Guid ConductorGuid { get; set; }
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
}

public sealed class ClienteListItemDto
{
    public int IdCliente { get; set; }
    public Guid ClienteGuid { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Nombre1 { get; set; } = string.Empty;
    public string? Nombre2 { get; set; }
    public string Apellido1 { get; set; } = string.Empty;
    public string? Apellido2 { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? DireccionPrincipal { get; set; }
    public string EstadoCliente { get; set; } = string.Empty;
    public long RowVersion { get; set; }
}

/// <summary>Respuesta de GET interno para MS.Reservas (nombres ya combinados).</summary>
public sealed class ClienteDetalleDto
{
    public int IdCliente { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}

public sealed class ConductorDetalleDto
{
    public int IdConductor { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public int EdadConductor { get; set; }
}
