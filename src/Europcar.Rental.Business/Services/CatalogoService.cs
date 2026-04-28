using Europcar.Rental.Business.DTOs.Response.Catalogos;
using Europcar.Rental.Business.DTOs.Request.Catalogos;
using Europcar.Rental.Business.Exceptions;
using Europcar.Rental.Business.Interfaces;
using Europcar.Rental.DataManagement.Interfaces;

namespace Europcar.Rental.Business.Services;

public class CatalogoService : ICatalogoService
{
    private static readonly string[] EstadosValidos = { "ACT", "INA" };
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
        var lista = await _localizacionDataService.GetAllAsync(soloActivas: true);
        return lista.Select(MapLocalizacion);
    }

    public async Task<LocalizacionResponse> GetLocalizacionByIdAsync(int id)
    {
        var l = await _localizacionDataService.GetByIdAsync(id)
            ?? throw new NotFoundException($"Localización con ID {id} no encontrada");
        return MapLocalizacion(l);
    }

    private static LocalizacionResponse MapLocalizacion(DataManagement.Models.LocalizacionModel l) => new()
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

    public async Task<IEnumerable<ExtraResponse>> GetExtrasAsync()
    {
        var lista = await _catalogoDataService.GetExtrasAsync();
        return lista.Select(MapExtra);
    }

    public async Task<ExtraResponse> GetExtraByIdAsync(int id)
    {
        var extra = await _catalogoDataService.GetExtraByIdAsync(id)
            ?? throw new NotFoundException($"Extra con ID {id} no encontrado");
        return MapExtra(extra);
    }

    public async Task<ExtraResponse> CreateExtraAsync(CrearExtraRequest request, string usuario)
    {
        ValidateExtra(request.CodigoExtra, request.NombreExtra, request.TipoExtra, request.ValorFijo);

        var codigo = request.CodigoExtra.Trim().ToUpper();
        var existente = await _catalogoDataService.GetExtraByCodigoAsync(codigo);
        if (existente != null)
            throw new ConflictException($"Ya existe un extra con código '{codigo}'");

        var model = new DataManagement.Models.CatalogoModel
        {
            Codigo = codigo,
            Nombre = request.NombreExtra.Trim(),
            Descripcion = request.DescripcionExtra?.Trim(),
            Tipo = request.TipoExtra.Trim().ToUpper(),
            RequiereStock = request.RequiereStock,
            ValorFijo = request.ValorFijo,
            Estado = "ACT"
        };

        var created = await _catalogoDataService.CreateExtraAsync(model, usuario);
        return MapExtra(created);
    }

    public async Task<ExtraResponse> UpdateExtraAsync(int id, ActualizarExtraRequest request, string usuario)
    {
        var existing = await _catalogoDataService.GetExtraByIdAsync(id)
            ?? throw new NotFoundException($"Extra con ID {id} no encontrado");

        ValidateExtra(existing.Codigo, request.NombreExtra, request.TipoExtra, request.ValorFijo);

        existing.Nombre = request.NombreExtra.Trim();
        existing.Descripcion = request.DescripcionExtra?.Trim();
        existing.Tipo = request.TipoExtra.Trim().ToUpper();
        existing.RequiereStock = request.RequiereStock;
        existing.ValorFijo = request.ValorFijo;

        await _catalogoDataService.UpdateExtraAsync(existing, usuario);
        var fresh = await _catalogoDataService.GetExtraByIdAsync(id);
        return MapExtra(fresh!);
    }

    public async Task CambiarEstadoExtraAsync(int id, CambiarEstadoExtraRequest request, string usuario)
    {
        var estado = (request.Estado ?? string.Empty).Trim().ToUpper();
        if (!EstadosValidos.Contains(estado))
            throw new BusinessException("Estado inválido. Use 'ACT' o 'INA'.");

        var existing = await _catalogoDataService.GetExtraByIdAsync(id)
            ?? throw new NotFoundException($"Extra con ID {id} no encontrado");

        await _catalogoDataService.UpdateExtraEstadoAsync(id, estado, usuario, request.Motivo);
        _ = existing;
    }

    public async Task DeleteExtraAsync(int id, string usuario)
    {
        var existing = await _catalogoDataService.GetExtraByIdAsync(id)
            ?? throw new NotFoundException($"Extra con ID {id} no encontrado");
        await _catalogoDataService.SoftDeleteExtraAsync(id, usuario);
        _ = existing;
    }

    private static void ValidateExtra(string codigo, string nombre, string tipo, decimal valorFijo)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Trim().Length > 20)
            throw new BusinessException("El código del extra es obligatorio (máximo 20 caracteres).");
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 100)
            throw new BusinessException("El nombre del extra es obligatorio (máximo 100 caracteres).");
        if (!string.IsNullOrWhiteSpace(tipo) && tipo.Trim().Length > 20)
            throw new BusinessException("El tipo del extra debe tener máximo 20 caracteres.");
        if (valorFijo < 0)
            throw new BusinessException("El valor fijo del extra no puede ser negativo.");
    }

    private static ExtraResponse MapExtra(DataManagement.Models.CatalogoModel c) => new()
    {
        IdExtra = c.Id,
        ExtraGuid = c.Guid,
        CodigoExtra = c.Codigo,
        NombreExtra = c.Nombre,
        DescripcionExtra = c.Descripcion,
        TipoExtra = c.Tipo ?? "SERVICIO",
        RequiereStock = c.RequiereStock,
        ValorFijo = c.ValorFijo ?? 0m,
        EstadoExtra = c.Estado
    };
}
