using System.Text.RegularExpressions;
using Europcar.Rental.Business.DTOs.Request.Localizaciones;
using Europcar.Rental.Business.DTOs.Response.Catalogos;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Interfaces;
using Europcar.Rental.DataManagement.Models;

namespace Europcar.Rental.Business.Services;

public class LocalizacionService : ILocalizacionService
{
    private static readonly string[] EstadosValidos = { "ACT", "INA" };
    private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    private readonly ILocalizacionDataService _localizacionDataService;
    private readonly ICiudadDataService _ciudadDataService;

    public LocalizacionService(
        ILocalizacionDataService localizacionDataService,
        ICiudadDataService ciudadDataService)
    {
        _localizacionDataService = localizacionDataService;
        _ciudadDataService = ciudadDataService;
    }

    public async Task<IEnumerable<LocalizacionResponse>> GetAllAsync(bool soloActivas = false)
    {
        var lista = await _localizacionDataService.GetAllAsync(soloActivas);
        return lista.Select(MapToResponse);
    }

    public async Task<LocalizacionResponse> GetByIdAsync(int id)
    {
        var l = await _localizacionDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Localización con ID {id} no encontrada");
        return MapToResponse(l);
    }

    public async Task<LocalizacionResponse> CreateAsync(CrearLocalizacionRequest request, string usuario)
    {
        await ValidateAsync(request.CodigoLocalizacion, request.NombreLocalizacion, request.IdCiudad,
            request.DireccionLocalizacion, request.TelefonoContacto, request.CorreoContacto, request.HorarioAtencion,
            request.Latitud, request.Longitud);

        var existente = await _localizacionDataService.GetByCodigoAsync(request.CodigoLocalizacion.Trim().ToUpper());
        if (existente != null)
            throw new ConflictException($"Ya existe una localización con código '{request.CodigoLocalizacion}'");

        var model = new LocalizacionModel
        {
            CodigoLocalizacion = request.CodigoLocalizacion,
            NombreLocalizacion = request.NombreLocalizacion,
            IdCiudad = request.IdCiudad,
            DireccionLocalizacion = request.DireccionLocalizacion,
            TelefonoContacto = request.TelefonoContacto,
            CorreoContacto = request.CorreoContacto,
            HorarioAtencion = request.HorarioAtencion,
            ZonaHoraria = request.ZonaHoraria ?? "America/Guayaquil",
            Latitud = request.Latitud,
            Longitud = request.Longitud
        };

        var created = await _localizacionDataService.CreateAsync(model, usuario);
        var fresh = await _localizacionDataService.GetByIdAsync(created.IdLocalizacion);
        return MapToResponse(fresh!);
    }

    public async Task<LocalizacionResponse> UpdateAsync(int id, ActualizarLocalizacionRequest request, string usuario)
    {
        var existente = await _localizacionDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Localización con ID {id} no encontrada");

        await ValidateAsync(existente.CodigoLocalizacion, request.NombreLocalizacion, request.IdCiudad,
            request.DireccionLocalizacion, request.TelefonoContacto, request.CorreoContacto, request.HorarioAtencion,
            request.Latitud, request.Longitud);

        existente.NombreLocalizacion = request.NombreLocalizacion;
        existente.IdCiudad = request.IdCiudad;
        existente.DireccionLocalizacion = request.DireccionLocalizacion;
        existente.TelefonoContacto = request.TelefonoContacto;
        existente.CorreoContacto = request.CorreoContacto;
        existente.HorarioAtencion = request.HorarioAtencion;
        existente.ZonaHoraria = request.ZonaHoraria ?? existente.ZonaHoraria;
        existente.Latitud = request.Latitud;
        existente.Longitud = request.Longitud;

        await _localizacionDataService.UpdateAsync(existente, usuario);

        var fresh = await _localizacionDataService.GetByIdAsync(id);
        return MapToResponse(fresh!);
    }

    public async Task CambiarEstadoAsync(int id, CambiarEstadoLocalizacionRequest request, string usuario)
    {
        var estado = (request.Estado ?? string.Empty).Trim().ToUpper();
        if (!EstadosValidos.Contains(estado))
            throw new BusinessException("Estado inválido. Use 'ACT' o 'INA'.");

        var existente = await _localizacionDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Localización con ID {id} no encontrada");

        await _localizacionDataService.UpdateEstadoAsync(id, estado, usuario, request.Motivo);
        _ = existente;
    }

    public async Task DeleteAsync(int id, string usuario)
    {
        var existente = await _localizacionDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Localización con ID {id} no encontrada");

        await _localizacionDataService.SoftDeleteAsync(id, usuario);
        _ = existente;
    }

    public async Task<IEnumerable<CiudadResponse>> GetCiudadesAsync()
    {
        var ciudades = await _ciudadDataService.GetAllAsync();
        return ciudades.Select(c => new CiudadResponse
        {
            IdCiudad = c.IdCiudad,
            CiudadGuid = c.CiudadGuid,
            IdPais = c.IdPais,
            NombreCiudad = c.NombreCiudad,
            NombrePais = c.NombrePais,
            EstadoCiudad = c.EstadoCiudad
        });
    }

    private async Task ValidateAsync(
        string codigo, string nombre, int idCiudad, string direccion, string telefono,
        string correo, string horario, decimal? latitud, decimal? longitud)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Length > 20)
            throw new BusinessException("El código de la localización es obligatorio (máximo 20 caracteres).");
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100)
            throw new BusinessException("El nombre de la localización es obligatorio (máximo 100 caracteres).");
        if (idCiudad <= 0)
            throw new BusinessException("Debe seleccionar una ciudad válida.");

        var ciudad = await _ciudadDataService.GetByIdAsync(idCiudad)
            ?? throw new BusinessException($"La ciudad con ID {idCiudad} no existe.");
        if (ciudad.EstadoCiudad != "ACT")
            throw new BusinessException("La ciudad seleccionada está inactiva.");

        if (string.IsNullOrWhiteSpace(direccion) || direccion.Length > 200)
            throw new BusinessException("La dirección es obligatoria (máximo 200 caracteres).");
        if (string.IsNullOrWhiteSpace(telefono) || telefono.Length > 20)
            throw new BusinessException("El teléfono de contacto es obligatorio (máximo 20 caracteres).");
        if (string.IsNullOrWhiteSpace(correo) || correo.Length > 120 || !EmailRegex.IsMatch(correo))
            throw new BusinessException("El correo de contacto es obligatorio y debe tener un formato válido.");
        if (string.IsNullOrWhiteSpace(horario) || horario.Length > 120)
            throw new BusinessException("El horario de atención es obligatorio (máximo 120 caracteres).");

        if (latitud.HasValue && (latitud < -90 || latitud > 90))
            throw new BusinessException("La latitud debe estar entre -90 y 90.");
        if (longitud.HasValue && (longitud < -180 || longitud > 180))
            throw new BusinessException("La longitud debe estar entre -180 y 180.");
    }

    private static LocalizacionResponse MapToResponse(LocalizacionModel l) => new()
    {
        IdLocalizacion = l.IdLocalizacion,
        LocalizacionGuid = l.LocalizacionGuid,
        CodigoLocalizacion = l.CodigoLocalizacion,
        NombreLocalizacion = l.NombreLocalizacion,
        IdCiudad = l.IdCiudad,
        DireccionLocalizacion = l.DireccionLocalizacion,
        TelefonoContacto = l.TelefonoContacto,
        CorreoContacto = l.CorreoContacto,
        HorarioAtencion = l.HorarioAtencion,
        ZonaHoraria = l.ZonaHoraria,
        Latitud = l.Latitud,
        Longitud = l.Longitud,
        NombreCiudad = l.NombreCiudad,
        EstadoLocalizacion = l.EstadoLocalizacion
    };
}
