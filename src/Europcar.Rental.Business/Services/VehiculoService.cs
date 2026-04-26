using Europcar.Rental.Business.DTOs.Request.Vehiculos;
using Europcar.Rental.Business.DTOs.Response.Vehiculos;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Common;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.Business.Services;

public class VehiculoService : IVehiculoService
{
    private readonly IVehiculoDataService _vehiculoDataService;
    private readonly IUnitOfWork _unitOfWork;

    // Valores válidos según constraints de la base de datos
    private static readonly string[] CombustiblesValidos = { "GASOLINA", "DIESEL", "HIBRIDO", "ELECTRICO" };
    private static readonly string[] TransmisionesValidas = { "AUTOMATICA", "MANUAL" };

    public VehiculoService(IVehiculoDataService vehiculoDataService, IUnitOfWork unitOfWork)
    {
        _vehiculoDataService = vehiculoDataService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<VehiculoResponse>> GetAllAsync()
    {
        var vehiculos = await _vehiculoDataService.GetAllAsync();
        return vehiculos.Select(MapToFullResponse);
    }

    public async Task<IEnumerable<VehiculoDisponibleResponse>> GetDisponiblesAsync(BuscarVehiculosRequest request)
    {
        var vehiculos = await _vehiculoDataService.GetDisponiblesAsync(request.LocalizacionId, request.CategoriaId);
        return vehiculos.Select(MapToDisponibleResponse);
    }

    public async Task<VehiculoResponse> GetByIdAsync(int id)
    {
        var vehiculo = await _vehiculoDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Vehículo con ID {id} no encontrado");
        return MapToFullResponse(vehiculo);
    }

    public async Task<VehiculoResponse> CreateAsync(CrearVehiculoRequest request)
    {
        // Validar campos de negocio
        ValidarCamposVehiculo(request.TipoCombustible, request.TipoTransmision,
            request.PrecioBaseDia, request.CapacidadPasajeros, request.NumeroPuertas, request.AnioFabricacion);

        // Validar placa duplicada
        var existente = await _vehiculoDataService.GetByPlacaAsync(request.PlacaVehiculo.ToUpper());
        if (existente != null)
            throw new ConflictException($"Ya existe un vehículo con placa {request.PlacaVehiculo}");

        var model = new VehiculoModel
        {
            CodigoInterno = $"VEH-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Placa = request.PlacaVehiculo.ToUpper(),
            IdMarca = request.IdMarca,
            IdCategoria = request.IdCategoria,
            Modelo = request.ModeloVehiculo,
            AnioFabricacion = request.AnioFabricacion,
            Color = request.ColorVehiculo,
            TipoCombustible = request.TipoCombustible.ToUpper(),
            TipoTransmision = request.TipoTransmision.ToUpper(),
            CapacidadPasajeros = request.CapacidadPasajeros,
            CapacidadMaletas = request.CapacidadMaletas,
            NumeroPuertas = request.NumeroPuertas,
            IdLocalizacion = request.IdLocalizacion,
            PrecioBaseDia = request.PrecioBaseDia,
            KilometrajeActual = request.KilometrajeActual,
            AireAcondicionado = request.AireAcondicionado,
            ObservacionesGenerales = request.ObservacionesGenerales,
            ImagenUrl = request.ImagenReferencialUrl
        };

        var created = await _vehiculoDataService.CreateAsync(model);
        await _unitOfWork.SaveChangesAsync();

        // Recargar para obtener los nombres de navegación (Marca, Categoría, Localización)
        var saved = await _vehiculoDataService.GetByIdAsync(created.IdVehiculo);
        return MapToFullResponse(saved!);
    }

    public async Task<VehiculoResponse> UpdateAsync(int id, ActualizarVehiculoRequest request)
    {
        var existing = await _vehiculoDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Vehículo con ID {id} no encontrado");

        // No permitir editar vehículos que estén alquilados o en curso
        if (existing.EstadoOperativo == "ALQUILADO")
            throw new BusinessException("No se puede editar un vehículo que está actualmente alquilado");

        // Validar campos de negocio
        ValidarCamposVehiculo(request.TipoCombustible, request.TipoTransmision,
            request.PrecioBaseDia, request.CapacidadPasajeros, request.NumeroPuertas, request.AnioFabricacion);

        // Validar placa duplicada si cambió
        if (!string.Equals(existing.Placa, request.PlacaVehiculo, StringComparison.OrdinalIgnoreCase))
        {
            var duplicada = await _vehiculoDataService.GetByPlacaAsync(request.PlacaVehiculo.ToUpper());
            if (duplicada != null)
                throw new ConflictException($"Ya existe un vehículo con placa {request.PlacaVehiculo}");
        }

        var model = new VehiculoModel
        {
            IdVehiculo = id,
            CodigoInterno = existing.CodigoInterno,
            Placa = request.PlacaVehiculo.ToUpper(),
            IdMarca = request.IdMarca,
            IdCategoria = request.IdCategoria,
            Modelo = request.ModeloVehiculo,
            AnioFabricacion = request.AnioFabricacion,
            Color = request.ColorVehiculo,
            TipoCombustible = request.TipoCombustible.ToUpper(),
            TipoTransmision = request.TipoTransmision.ToUpper(),
            CapacidadPasajeros = request.CapacidadPasajeros,
            CapacidadMaletas = request.CapacidadMaletas,
            NumeroPuertas = request.NumeroPuertas,
            IdLocalizacion = request.IdLocalizacion,
            PrecioBaseDia = request.PrecioBaseDia,
            KilometrajeActual = request.KilometrajeActual,
            AireAcondicionado = request.AireAcondicionado,
            ObservacionesGenerales = request.ObservacionesGenerales,
            ImagenUrl = request.ImagenReferencialUrl,
            RowVersion = request.RowVersion
        };

        await _vehiculoDataService.UpdateAsync(model);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _vehiculoDataService.GetByIdAsync(id);
        return MapToFullResponse(updated!);
    }

    public async Task DeleteAsync(int id, string usuario)
    {
        var existing = await _vehiculoDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Vehículo con ID {id} no encontrado");

        // No permitir eliminar vehículos con reservas activas o contratos abiertos
        if (existing.EstadoOperativo is "ALQUILADO" or "RESERVADO")
            throw new BusinessException("No se puede eliminar un vehículo que tiene reservas activas o está alquilado");

        await _vehiculoDataService.SoftDeleteAsync(id, usuario);
        await _unitOfWork.SaveChangesAsync();
    }

    private static void ValidarCamposVehiculo(string combustible, string transmision,
        decimal precioDia, short capacidadPasajeros, short numeroPuertas, short anio)
    {
        if (!CombustiblesValidos.Contains(combustible.ToUpper()))
            throw new BusinessException($"Tipo de combustible inválido. Valores permitidos: {string.Join(", ", CombustiblesValidos)}");

        if (!TransmisionesValidas.Contains(transmision.ToUpper()))
            throw new BusinessException($"Tipo de transmisión inválido. Valores permitidos: {string.Join(", ", TransmisionesValidas)}");

        if (precioDia <= 0)
            throw new BusinessException("El precio base por día debe ser mayor a 0");

        if (capacidadPasajeros <= 0)
            throw new BusinessException("La capacidad de pasajeros debe ser mayor a 0");

        if (numeroPuertas <= 0)
            throw new BusinessException("El número de puertas debe ser mayor a 0");

        var currentYear = (short)(DateTime.UtcNow.Year + 1);
        if (anio < 1990 || anio > currentYear)
            throw new BusinessException($"El año de fabricación debe estar entre 1990 y {currentYear}");
    }

    private static VehiculoResponse MapToFullResponse(VehiculoModel v) => new()
    {
        IdVehiculo = v.IdVehiculo,
        VehiculoGuid = v.VehiculoGuid,
        CodigoInterno = v.CodigoInterno,
        Placa = v.Placa,
        IdMarca = v.IdMarca,
        Marca = v.Marca,
        IdCategoria = v.IdCategoria,
        Categoria = v.Categoria,
        Modelo = v.Modelo,
        AnioFabricacion = v.AnioFabricacion,
        Color = v.Color,
        TipoCombustible = v.TipoCombustible,
        TipoTransmision = v.TipoTransmision,
        CapacidadPasajeros = v.CapacidadPasajeros,
        CapacidadMaletas = v.CapacidadMaletas,
        NumeroPuertas = v.NumeroPuertas,
        PrecioBaseDia = v.PrecioBaseDia,
        KilometrajeActual = v.KilometrajeActual,
        AireAcondicionado = v.AireAcondicionado,
        EstadoOperativo = v.EstadoOperativo,
        ObservacionesGenerales = v.ObservacionesGenerales,
        ImagenReferencialUrl = v.ImagenUrl,
        IdLocalizacion = v.IdLocalizacion,
        Localizacion = v.NombreLocalizacion,
        EstadoVehiculo = v.EstadoVehiculo,
        RowVersion = v.RowVersion
    };

    private static VehiculoDisponibleResponse MapToDisponibleResponse(VehiculoModel v) => new()
    {
        IdVehiculo = v.IdVehiculo,
        VehiculoGuid = v.VehiculoGuid,
        CodigoInterno = v.CodigoInterno,
        Placa = v.Placa,
        Marca = v.Marca,
        Categoria = v.Categoria,
        Modelo = v.Modelo,
        AnioFabricacion = v.AnioFabricacion,
        Color = v.Color,
        TipoCombustible = v.TipoCombustible,
        TipoTransmision = v.TipoTransmision,
        CapacidadPasajeros = v.CapacidadPasajeros,
        CapacidadMaletas = v.CapacidadMaletas,
        PrecioBaseDia = v.PrecioBaseDia,
        AireAcondicionado = v.AireAcondicionado,
        ImagenUrl = v.ImagenUrl,
        IdLocalizacion = v.IdLocalizacion,
        Localizacion = v.NombreLocalizacion
    };
}
