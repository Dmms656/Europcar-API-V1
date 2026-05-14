using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Models.Common;
using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.Business.Interfaces;

namespace Middleware.RedCar.Api.Controllers.V2.Booking;

/// <summary>
/// Endpoints 4 y 5 del contrato (listado y detalle de localizaciones).
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/booking/localizaciones")]
[Produces("application/json")]
public sealed class LocalizacionesBookingController : ControllerBase
{
    private readonly IMarketplaceOrchestrator _marketplace;

    public LocalizacionesBookingController(IMarketplaceOrchestrator marketplace)
    {
        _marketplace = marketplace;
    }

    /// <summary>
    /// Endpoint 4 - Listar localizaciones.
    /// GET /api/v2/booking/localizaciones
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<LocalizacionesListaPayload>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<ActionResult<ApiResponse<LocalizacionesListaPayload>>> Listar(
        [FromQuery] int? idCiudad,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var (items, paginacion) = await _marketplace.ListLocalizacionesAsync(idCiudad, page, limit, ct);

        var payload = new LocalizacionesListaPayload
        {
            Localizaciones = items,
            Paginacion = paginacion,
            _Links = BuildLinks("/api/v2/booking/localizaciones", paginacion, idCiudad)
        };
        return Ok(ApiResponse<LocalizacionesListaPayload>.Ok(payload));
    }

    /// <summary>
    /// Endpoint 5 - Detalle de una localizacion.
    /// GET /api/v2/booking/localizaciones/{idLocalizacion}
    /// </summary>
    [HttpGet("{idLocalizacion:int}")]
    [ProducesResponseType(typeof(ApiResponse<LocalizacionDetallePayload>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<ActionResult<ApiResponse<LocalizacionDetallePayload>>> Detalle(
        [FromRoute] int idLocalizacion,
        CancellationToken ct)
    {
        var loc = await _marketplace.GetLocalizacionAsync(idLocalizacion, ct);
        return Ok(ApiResponse<LocalizacionDetallePayload>.Ok(new LocalizacionDetallePayload { Localizacion = loc }));
    }

    private static Dictionary<string, LinkHref> BuildLinks(string basePath, PaginacionResponse p, int? idCiudad)
    {
        var qs = new List<string>
        {
            $"page={p.PaginaActual}",
            $"limit={p.ElementosPorPagina}"
        };
        if (idCiudad.HasValue)
            qs.Insert(0, $"idCiudad={idCiudad.Value}");

        var qstr = string.Join('&', qs);
        var links = new Dictionary<string, LinkHref>
        {
            ["self"] = new() { Href = $"{basePath}?{qstr}" }
        };
        if (p.PaginaActual < p.TotalPaginas)
        {
            var qsNext = new List<string> { $"page={p.PaginaActual + 1}", $"limit={p.ElementosPorPagina}" };
            if (idCiudad.HasValue) qsNext.Insert(0, $"idCiudad={idCiudad.Value}");
            links["next"] = new() { Href = $"{basePath}?{string.Join('&', qsNext)}" };
        }
        if (p.PaginaActual > 1)
        {
            var qsPrev = new List<string> { $"page={p.PaginaActual - 1}", $"limit={p.ElementosPorPagina}" };
            if (idCiudad.HasValue) qsPrev.Insert(0, $"idCiudad={idCiudad.Value}");
            links["prev"] = new() { Href = $"{basePath}?{string.Join('&', qsPrev)}" };
        }
        return links;
    }
}

public sealed class LocalizacionesListaPayload
{
    public IReadOnlyList<LocalizacionBookingResponse> Localizaciones { get; set; } = Array.Empty<LocalizacionBookingResponse>();
    public PaginacionResponse Paginacion { get; set; } = new();

    [JsonPropertyName("_links")]
    public Dictionary<string, LinkHref> _Links { get; set; } = new();
}

public sealed class LocalizacionDetallePayload
{
    public LocalizacionBookingResponse Localizacion { get; set; } = new();
}
