using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Compatibility;
using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.Business.Interfaces;

namespace Middleware.RedCar.Api.Controllers.V1;

/// <summary>
/// Rutas /api/v1/localizaciones, /ciudades, /categorias, /extras (monolito <c>BookingCatalogosController</c>).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}")]
[Produces("application/json")]
public sealed class LegacyPublicCatalogosController : ControllerBase
{
    private readonly IMarketplaceOrchestrator _marketplace;

    public LegacyPublicCatalogosController(IMarketplaceOrchestrator marketplace)
    {
        _marketplace = marketplace;
    }

    [HttpGet("localizaciones")]
    public async Task<IActionResult> Localizaciones([FromQuery] int? idCiudad, [FromQuery] int page = 1, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var (items, paginacion) = await _marketplace.ListLocalizacionesAsync(idCiudad, page, limit, ct);
        var mapped = items.Select(LegacyV1DtoMapper.ToLocalizacionListItem).ToList();
        var data = new
        {
            localizaciones = mapped,
            paginacion,
            _links = BuildLocLinks(idCiudad, paginacion)
        };
        return Ok(LegacyBookingEnvelope.Ok(data));
    }

    [HttpGet("localizaciones/{id:int}")]
    public async Task<IActionResult> LocalizacionDetalle([FromRoute] int id, CancellationToken ct)
    {
        var loc = await _marketplace.GetLocalizacionAsync(id, ct);
        return Ok(LegacyBookingEnvelope.Ok(new { localizacion = LegacyV1DtoMapper.ToLocalizacionListItem(loc) }));
    }

    [HttpGet("ciudades")]
    public async Task<IActionResult> Ciudades(CancellationToken ct)
    {
        var ciudades = await _marketplace.ListCiudadesAsync(ct);
        return Ok(LegacyBookingEnvelope.Ok(new { ciudades }));
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> Categorias([FromQuery] int page = 1, [FromQuery] int limit = 200, CancellationToken ct = default)
    {
        var (items, paginacion) = await _marketplace.ListCategoriasAsync(page, limit, ct);
        var mapped = items.Select(LegacyV1DtoMapper.ToCategoriaItem).ToList();
        return Ok(LegacyBookingEnvelope.Ok(new { categorias = mapped, paginacion }));
    }

    [HttpGet("extras")]
    public async Task<IActionResult> Extras([FromQuery] int page = 1, [FromQuery] int limit = 100, CancellationToken ct = default)
    {
        var (items, paginacion) = await _marketplace.ListExtrasAsync(page, limit, ct);
        var mapped = items.Select(LegacyV1DtoMapper.ToExtraItem).ToList();
        return Ok(LegacyBookingEnvelope.Ok(new { extras = mapped, paginacion }));
    }

    private static Dictionary<string, LinkHref> BuildLocLinks(int? idCiudad, PaginacionResponse p)
    {
        var basePath = "/api/v1/localizaciones";
        var qs = new List<string> { $"page={p.PaginaActual}", $"limit={p.ElementosPorPagina}" };
        if (idCiudad.HasValue) qs.Insert(0, $"idCiudad={idCiudad.Value}");
        var qstr = string.Join('&', qs);
        var links = new Dictionary<string, LinkHref> { ["self"] = new() { Href = $"{basePath}?{qstr}" } };
        if (p.PaginaActual < p.TotalPaginas)
        {
            var n = new List<string> { $"page={p.PaginaActual + 1}", $"limit={p.ElementosPorPagina}" };
            if (idCiudad.HasValue) n.Insert(0, $"idCiudad={idCiudad.Value}");
            links["next"] = new() { Href = $"{basePath}?{string.Join('&', n)}" };
        }
        return links;
    }
}
