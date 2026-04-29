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

    public async Task<IEnumerable<CatalogoResponse>> GetPaisesAsync()
    {
        var lista = await _catalogoDataService.GetPaisesAsync();
        return lista.Select(MapCatalogo);
    }

    public async Task<CatalogoResponse> GetPaisByIdAsync(int id)
    {
        var p = await _catalogoDataService.GetPaisByIdAsync(id)
            ?? throw new NotFoundException($"País con ID {id} no encontrado");
        return MapCatalogo(p);
    }

    public async Task<CatalogoResponse> CreatePaisAsync(CrearPaisRequest request, string usuario)
    {
        var codigo = (request.CodigoIso2 ?? string.Empty).Trim().ToUpper();
        var nombre = (request.NombrePais ?? string.Empty).Trim();
        ValidatePais(codigo, nombre);

        var existente = await _catalogoDataService.GetPaisByCodigoIso2Async(codigo);
        if (existente != null)
            throw new ConflictException($"Ya existe un país con código '{codigo}'");

        var model = await _catalogoDataService.CreatePaisAsync(new DataManagement.Models.CatalogoModel
        {
            Codigo = codigo,
            Nombre = nombre,
            Estado = "ACT"
        }, usuario);

        return MapCatalogo(model);
    }

    public async Task<CatalogoResponse> UpdatePaisAsync(int id, ActualizarPaisRequest request, string usuario)
    {
        var existing = await _catalogoDataService.GetPaisByIdAsync(id)
            ?? throw new NotFoundException($"País con ID {id} no encontrado");
        var nombre = (request.NombrePais ?? string.Empty).Trim();
        ValidatePais(existing.Codigo, nombre);
        existing.Nombre = nombre;
        await _catalogoDataService.UpdatePaisAsync(existing, usuario);
        var fresh = await _catalogoDataService.GetPaisByIdAsync(id);
        return MapCatalogo(fresh!);
    }

    public async Task CambiarEstadoPaisAsync(int id, CambiarEstadoPaisRequest request, string usuario)
    {
        var estado = (request.Estado ?? string.Empty).Trim().ToUpper();
        if (!EstadosValidos.Contains(estado))
            throw new BusinessException("Estado inválido. Use 'ACT' o 'INA'.");

        var existing = await _catalogoDataService.GetPaisByIdAsync(id)
            ?? throw new NotFoundException($"País con ID {id} no encontrado");

        await _catalogoDataService.UpdatePaisEstadoAsync(id, estado, usuario, request.Motivo);
        _ = existing;
    }

    public async Task DeletePaisAsync(int id, string usuario)
    {
        var existing = await _catalogoDataService.GetPaisByIdAsync(id)
            ?? throw new NotFoundException($"País con ID {id} no encontrado");
        await _catalogoDataService.SoftDeletePaisAsync(id, usuario);
        _ = existing;
    }

    public async Task<IEnumerable<CiudadResponse>> GetCiudadesAsync()
    {
        var lista = await _catalogoDataService.GetCiudadesAsync();
        return lista.Select(MapCiudad);
    }

    public async Task<CiudadResponse> GetCiudadByIdAsync(int id)
    {
        var ciudad = await _catalogoDataService.GetCiudadByIdAsync(id)
            ?? throw new NotFoundException($"Ciudad con ID {id} no encontrada");
        return MapCiudad(ciudad);
    }

    public async Task<CiudadResponse> CreateCiudadAsync(CrearCiudadRequest request, string usuario)
    {
        ValidateCiudad(request.IdPais, request.NombreCiudad);
        var pais = await _catalogoDataService.GetPaisByIdAsync(request.IdPais)
            ?? throw new BusinessException($"No existe país con ID {request.IdPais}");
        if (pais.Estado != "ACT")
            throw new BusinessException("El país seleccionado está inactivo.");

        var created = await _catalogoDataService.CreateCiudadAsync(new DataManagement.Models.CiudadModel
        {
            IdPais = request.IdPais,
            NombreCiudad = request.NombreCiudad.Trim(),
            EstadoCiudad = "ACT"
        }, usuario);

        var fresh = await _catalogoDataService.GetCiudadByIdAsync(created.IdCiudad);
        return MapCiudad(fresh!);
    }

    public async Task<CiudadResponse> UpdateCiudadAsync(int id, ActualizarCiudadRequest request, string usuario)
    {
        var existing = await _catalogoDataService.GetCiudadByIdAsync(id)
            ?? throw new NotFoundException($"Ciudad con ID {id} no encontrada");
        ValidateCiudad(request.IdPais, request.NombreCiudad);
        var pais = await _catalogoDataService.GetPaisByIdAsync(request.IdPais)
            ?? throw new BusinessException($"No existe país con ID {request.IdPais}");
        if (pais.Estado != "ACT")
            throw new BusinessException("El país seleccionado está inactivo.");

        existing.IdPais = request.IdPais;
        existing.NombreCiudad = request.NombreCiudad.Trim();
        await _catalogoDataService.UpdateCiudadAsync(existing, usuario);
        var fresh = await _catalogoDataService.GetCiudadByIdAsync(id);
        return MapCiudad(fresh!);
    }

    public async Task CambiarEstadoCiudadAsync(int id, CambiarEstadoCiudadRequest request, string usuario)
    {
        var estado = (request.Estado ?? string.Empty).Trim().ToUpper();
        if (!EstadosValidos.Contains(estado))
            throw new BusinessException("Estado inválido. Use 'ACT' o 'INA'.");

        var existing = await _catalogoDataService.GetCiudadByIdAsync(id)
            ?? throw new NotFoundException($"Ciudad con ID {id} no encontrada");

        await _catalogoDataService.UpdateCiudadEstadoAsync(id, estado, usuario, request.Motivo);
        _ = existing;
    }

    public async Task DeleteCiudadAsync(int id, string usuario)
    {
        var existing = await _catalogoDataService.GetCiudadByIdAsync(id)
            ?? throw new NotFoundException($"Ciudad con ID {id} no encontrada");
        await _catalogoDataService.SoftDeleteCiudadAsync(id, usuario);
        _ = existing;
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

    private static CatalogoResponse MapCatalogo(DataManagement.Models.CatalogoModel c) => new()
    {
        Id = c.Id,
        Guid = c.Guid,
        Codigo = c.Codigo,
        Nombre = c.Nombre,
        Descripcion = c.Descripcion,
        Estado = c.Estado
    };

    private static CiudadResponse MapCiudad(DataManagement.Models.CiudadModel c) => new()
    {
        IdCiudad = c.IdCiudad,
        CiudadGuid = c.CiudadGuid,
        IdPais = c.IdPais,
        NombreCiudad = c.NombreCiudad,
        NombrePais = c.NombrePais,
        EstadoCiudad = c.EstadoCiudad
    };

    private static void ValidatePais(string codigoIso2, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigoIso2) || codigoIso2.Length != 2)
            throw new BusinessException("El código ISO2 del país debe tener exactamente 2 caracteres.");
        if (!codigoIso2.All(char.IsLetter))
            throw new BusinessException("El código ISO2 del país solo debe contener letras.");
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100)
            throw new BusinessException("El nombre del país es obligatorio (máximo 100 caracteres).");
    }

    private static void ValidateCiudad(int idPais, string nombreCiudad)
    {
        if (idPais <= 0)
            throw new BusinessException("Debe seleccionar un país válido.");
        if (string.IsNullOrWhiteSpace(nombreCiudad) || nombreCiudad.Trim().Length > 120)
            throw new BusinessException("El nombre de la ciudad es obligatorio (máximo 120 caracteres).");
    }
}
