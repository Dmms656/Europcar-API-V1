using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.Api.Services;

/// <summary>
/// Alta/búsqueda de cliente invitado para el flujo público de reserva (sin filtrar PII en respuestas).
/// </summary>
public sealed class GuestClientService
{
    private readonly IClienteDataService _clienteDataService;

    public GuestClientService(IClienteDataService clienteDataService)
    {
        _clienteDataService = clienteDataService;
    }

    public async Task<(int IdCliente, bool EsNuevo)> ResolveGuestClientAsync(
        string cedula,
        string nombre,
        string? apellido,
        string? telefono,
        string? correo,
        string? direccion)
    {
        var existing = await _clienteDataService.GetByIdentificacionAsync(cedula.Trim());
        if (existing != null)
            return (existing.IdCliente, false);

        var codigo = $"CLT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var newCliente = await _clienteDataService.CreateAsync(new ClienteModel
        {
            CodigoCliente = codigo,
            TipoIdentificacion = "CED",
            NumeroIdentificacion = cedula.Trim(),
            Nombre1 = nombre.Trim(),
            Apellido1 = apellido?.Trim() ?? "",
            Telefono = telefono?.Trim() ?? "",
            Correo = correo?.Trim() ?? "",
            DireccionPrincipal = direccion?.Trim(),
            FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-25))
        });

        return (newCliente.IdCliente, true);
    }
}
