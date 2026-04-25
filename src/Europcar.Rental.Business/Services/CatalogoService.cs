using Europcar.Rental.Business.DTOs.Response.Catalogos;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Interfaces;

namespace Europcar.Rental.Business.Services;

public class CatalogoService : ICatalogoService
{
    private readonly ILocalizacionDataService _localizacionDataService;
    private readonly ICatalogoDataService _catalogoDataService;

    public CatalogoService(
        ILocalizacionDataService localizacionDataService,
        ICatalogoDataService catalogoDataService)
    {
        _localizacionDataService = localizacionDataService;
        _catalogoDataService = catalogoDataService;
    }

    public async Task<IEnumerable<LocalizacionResponse>> GetLocalizacionesAsync()
    {
        var lista = await _localizacionDataService.GetAllAsync();
        return lista.Select(l => new LocalizacionResponse
        {
            IdLocalizacion = l.IdLocalizacion,
            LocalizacionGuid = l.LocalizacionGuid,
            CodigoLocalizacion = l.CodigoLocalizacion,
            NombreLocalizacion = l.NombreLocalizacion,
            DireccionLocalizacion = l.DireccionLocalizacion,
            TelefonoContacto = l.TelefonoContacto,
            CorreoContacto = l.CorreoContacto,
            HorarioAtencion = l.HorarioAtencion,
            NombreCiudad = l.NombreCiudad,
            EstadoLocalizacion = l.EstadoLocalizacion
        });
    }

    public async Task<LocalizacionResponse> GetLocalizacionByIdAsync(int id)
    {
        var l = await _localizacionDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Localización con ID {id} no encontrada");

        return new LocalizacionResponse
        {
            IdLocalizacion = l.IdLocalizacion,
            LocalizacionGuid = l.LocalizacionGuid,
            CodigoLocalizacion = l.CodigoLocalizacion,
            NombreLocalizacion = l.NombreLocalizacion,
            DireccionLocalizacion = l.DireccionLocalizacion,
            TelefonoContacto = l.TelefonoContacto,
            CorreoContacto = l.CorreoContacto,
            HorarioAtencion = l.HorarioAtencion,
            NombreCiudad = l.NombreCiudad,
            EstadoLocalizacion = l.EstadoLocalizacion
        };
    }

    public async Task<IEnumerable<CatalogoResponse>> GetCategoriasAsync()
    {
        var lista = await _catalogoDataService.GetCategoriasAsync();
        return lista.Select(c => new CatalogoResponse
        {
            Id = c.Id, Guid = c.Guid, Codigo = c.Codigo,
            Nombre = c.Nombre, Descripcion = c.Descripcion, Estado = c.Estado
        });
    }

    public async Task<IEnumerable<CatalogoResponse>> GetMarcasAsync()
    {
        var lista = await _catalogoDataService.GetMarcasAsync();
        return lista.Select(c => new CatalogoResponse
        {
            Id = c.Id, Guid = c.Guid, Codigo = c.Codigo,
            Nombre = c.Nombre, Descripcion = c.Descripcion, Estado = c.Estado
        });
    }

    public async Task<IEnumerable<CatalogoResponse>> GetExtrasAsync()
    {
        var lista = await _catalogoDataService.GetExtrasAsync();
        return lista.Select(c => new CatalogoResponse
        {
            Id = c.Id, Guid = c.Guid, Codigo = c.Codigo,
            Nombre = c.Nombre, Descripcion = c.Descripcion, Estado = c.Estado
        });
    }
}
