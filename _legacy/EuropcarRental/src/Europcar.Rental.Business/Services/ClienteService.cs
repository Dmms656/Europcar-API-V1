using Europcar.Rental.Business.DTOs.Request.Clientes;
using Europcar.Rental.Business.DTOs.Response.Clientes;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Common;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.Business.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteDataService _clienteDataService;
    private readonly IUnitOfWork _unitOfWork;

    public ClienteService(IClienteDataService clienteDataService, IUnitOfWork unitOfWork)
    {
        _clienteDataService = clienteDataService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ClienteResponse>> GetAllAsync()
    {
        var clientes = await _clienteDataService.GetAllAsync();
        return clientes.Select(MapToResponse);
    }

    public async Task<ClienteResponse> GetByIdAsync(int id)
    {
        var cliente = await _clienteDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Cliente con ID {id} no encontrado");
        return MapToResponse(cliente);
    }

    public async Task<ClienteResponse> CreateAsync(CrearClienteRequest request)
    {
        // Validar edad mínima (18 años)
        var edad = DateOnly.FromDateTime(DateTime.Today).Year - request.FechaNacimiento.Year;
        if (edad < 18)
            throw new BusinessException("El cliente debe ser mayor de 18 años");

        // Validar duplicado de identificación
        var existente = await _clienteDataService.GetByIdentificacionAsync(request.NumeroIdentificacion);
        if (existente != null)
            throw new ConflictException($"Ya existe un cliente con identificación {request.NumeroIdentificacion}");

        var model = new ClienteModel
        {
            CodigoCliente = $"CLI-{DateTime.UtcNow:yyyyMMddHHmmss}",
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

        var created = await _clienteDataService.CreateAsync(model);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(created);
    }

    public async Task<ClienteResponse> UpdateAsync(int id, ActualizarClienteRequest request)
    {
        var existing = await _clienteDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Cliente con ID {id} no encontrado");

        var model = new ClienteModel
        {
            IdCliente = id,
            CodigoCliente = existing.CodigoCliente,
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

        await _clienteDataService.UpdateAsync(model);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _clienteDataService.GetByIdAsync(id);
        return MapToResponse(updated!);
    }

    public async Task DeleteAsync(int id, string usuario)
    {
        _ = await _clienteDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Cliente con ID {id} no encontrado");

        await _clienteDataService.SoftDeleteAsync(id, usuario);
        await _unitOfWork.SaveChangesAsync();
    }

    private static ClienteResponse MapToResponse(ClienteModel m) => new()
    {
        IdCliente = m.IdCliente,
        ClienteGuid = m.ClienteGuid,
        CodigoCliente = m.CodigoCliente,
        TipoIdentificacion = m.TipoIdentificacion,
        NumeroIdentificacion = m.NumeroIdentificacion,
        NombreCompleto = $"{m.Nombre1} {m.Nombre2 ?? ""} {m.Apellido1} {m.Apellido2 ?? ""}".Trim(),
        Nombre1 = m.Nombre1,
        Nombre2 = m.Nombre2,
        Apellido1 = m.Apellido1,
        Apellido2 = m.Apellido2,
        FechaNacimiento = m.FechaNacimiento,
        Telefono = m.Telefono,
        Correo = m.Correo,
        DireccionPrincipal = m.DireccionPrincipal,
        EstadoCliente = m.EstadoCliente,
        RowVersion = m.RowVersion
    };
}
