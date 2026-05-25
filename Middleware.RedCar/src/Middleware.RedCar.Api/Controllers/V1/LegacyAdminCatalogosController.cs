using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Compatibility;
using Middleware.RedCar.DataAccess.Clients.Interfaces;
using RedCar.Shared.Contracts.Common;

namespace Middleware.RedCar.Api.Controllers.V1;

/// <summary>Catálogo administrativo: <c>/api/v1/Catalogos/*</c> (lectura desde microservicios).</summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/Catalogos")]
[Produces("application/json")]
public sealed class LegacyAdminCatalogosController : ControllerBase
{
    private readonly ICatalogoClient _catalogo;
    private readonly ILocalizacionesClient _localizaciones;

    public LegacyAdminCatalogosController(ICatalogoClient catalogo, ILocalizacionesClient localizaciones)
    {
        _catalogo = catalogo;
        _localizaciones = localizaciones;
    }

    [HttpGet("paises")]
    public async Task<IActionResult> GetPaises(CancellationToken ct)
    {
        var items = await _localizaciones.ListPaisesAsync(ct) ?? Array.Empty<PaisDto>();
        return Ok(ApiResponse<object>.Ok(items.Select(LegacyAdminDtoMapper.ToPais).ToList(), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("paises/{id:int}")]
    public async Task<IActionResult> GetPaisById(int id, CancellationToken ct)
    {
        var items = await _localizaciones.ListPaisesAsync(ct) ?? Array.Empty<PaisDto>();
        var pais = items.FirstOrDefault(p => p.Id == id);
        return pais is null
            ? NotFound(ApiResponse<object>.Fail(404, "País no encontrado", HttpContext.TraceIdentifier))
            : Ok(ApiResponse<object>.Ok(LegacyAdminDtoMapper.ToPais(pais), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("ciudades")]
    public async Task<IActionResult> GetCiudades(CancellationToken ct)
    {
        var items = await _localizaciones.ListCiudadesAsync(ct) ?? Array.Empty<CiudadDto>();
        return Ok(ApiResponse<object>.Ok(items.Select(LegacyAdminDtoMapper.ToCiudad).ToList(), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("ciudades/{id:int}")]
    public async Task<IActionResult> GetCiudadById(int id, CancellationToken ct)
    {
        var items = await _localizaciones.ListCiudadesAsync(ct) ?? Array.Empty<CiudadDto>();
        var ciudad = items.FirstOrDefault(c => c.IdCiudad == id);
        return ciudad is null
            ? NotFound(ApiResponse<object>.Fail(404, "Ciudad no encontrada", HttpContext.TraceIdentifier))
            : Ok(ApiResponse<object>.Ok(LegacyAdminDtoMapper.ToCiudad(ciudad), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("localizaciones")]
    public async Task<IActionResult> GetLocalizaciones(CancellationToken ct)
    {
        var all = await ListAllLocalizacionesAsync(ct);
        return Ok(ApiResponse<object>.Ok(all, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("localizaciones/{id:int}")]
    public async Task<IActionResult> GetLocalizacionById(int id, CancellationToken ct)
    {
        var loc = await _localizaciones.GetLocalizacionAsync(id, ct);
        return loc is null
            ? NotFound(ApiResponse<object>.Fail(404, "Localización no encontrada", HttpContext.TraceIdentifier))
            : Ok(ApiResponse<object>.Ok(LegacyAdminDtoMapper.ToLocalizacion(loc), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias(CancellationToken ct)
    {
        var paged = await _catalogo.ListCategoriasAsync(1, 500, ct);
        var items = paged?.Items ?? Array.Empty<CategoriaDto>();
        return Ok(ApiResponse<object>.Ok(items.Select(LegacyAdminDtoMapper.ToCategoria).ToList(), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("marcas")]
    public async Task<IActionResult> GetMarcas(CancellationToken ct)
    {
        var items = await _catalogo.ListMarcasAsync(ct) ?? Array.Empty<MarcaDto>();
        return Ok(ApiResponse<object>.Ok(items.Select(LegacyAdminDtoMapper.ToMarca).ToList(), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("extras")]
    public async Task<IActionResult> GetExtras(CancellationToken ct)
    {
        var paged = await _catalogo.ListExtrasAsync(1, 500, ct);
        var items = paged?.Items ?? Array.Empty<ExtraDto>();
        return Ok(ApiResponse<object>.Ok(items.Select(LegacyAdminDtoMapper.ToExtra).ToList(), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("extras/{id:int}")]
    public async Task<IActionResult> GetExtraById(int id, CancellationToken ct)
    {
        var paged = await _catalogo.ListExtrasAsync(1, 500, ct);
        var extra = paged?.Items.FirstOrDefault(e => e.IdExtra == id);
        return extra is null
            ? NotFound(ApiResponse<object>.Fail(404, "Extra no encontrado", HttpContext.TraceIdentifier))
            : Ok(ApiResponse<object>.Ok(LegacyAdminDtoMapper.ToExtra(extra), traceId: HttpContext.TraceIdentifier));
    }

    [HttpPost("paises")]
    [HttpPut("paises/{id:int}")]
    [HttpPut("paises/{id:int}/estado")]
    [HttpDelete("paises/{id:int}")]
    [HttpPost("ciudades")]
    [HttpPut("ciudades/{id:int}")]
    [HttpPut("ciudades/{id:int}/estado")]
    [HttpDelete("ciudades/{id:int}")]
    [HttpPost("extras")]
    [HttpPut("extras/{id:int}")]
    [HttpPut("extras/{id:int}/estado")]
    [HttpDelete("extras/{id:int}")]
    public IActionResult NotImplementedWrite()
        => StatusCode(501, ApiResponse<object>.Fail(501, "Operación de escritura no implementada en middleware; use el monolito o extienda el microservicio.", HttpContext.TraceIdentifier));

    private async Task<List<object>> ListAllLocalizacionesAsync(CancellationToken ct)
    {
        var result = new List<object>();
        var page = 1;
        while (page <= 20)
        {
            var paged = await _localizaciones.ListLocalizacionesAsync(null, page, 100, ct);
            if (paged?.Items is null || paged.Items.Count == 0) break;
            result.AddRange(paged.Items.Select(LegacyAdminDtoMapper.ToLocalizacion));
            if (paged.Items.Count < 100) break;
            page++;
        }
        return result;
    }
}
