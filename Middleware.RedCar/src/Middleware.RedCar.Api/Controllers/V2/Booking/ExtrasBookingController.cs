using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Models.Common;
using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.Business.Interfaces;

namespace Middleware.RedCar.Api.Controllers.V2.Booking;

/// <summary>
/// Endpoint 7 del contrato - Listar extras disponibles.
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/booking/extras")]
[Produces("application/json")]
public sealed class ExtrasBookingController : ControllerBase
{
    private readonly IMarketplaceOrchestrator _marketplace;

    public ExtrasBookingController(IMarketplaceOrchestrator marketplace)
    {
        _marketplace = marketplace;
    }

    /// <summary>
    /// GET /api/v2/booking/extras
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ExtrasListaPayload>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<ActionResult<ApiResponse<ExtrasListaPayload>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var (items, paginacion) = await _marketplace.ListExtrasAsync(page, limit, ct);

        var payload = new ExtrasListaPayload
        {
            Extras = items,
            Paginacion = paginacion,
            _Links = BuildLinks(paginacion)
        };
        return Ok(ApiResponse<ExtrasListaPayload>.Ok(payload));
    }

    private static Dictionary<string, LinkHref> BuildLinks(PaginacionResponse p)
    {
        var basePath = "/api/v2/booking/extras";
        var links = new Dictionary<string, LinkHref>
        {
            ["self"] = new() { Href = $"{basePath}?page={p.PaginaActual}&limit={p.ElementosPorPagina}" }
        };
        if (p.PaginaActual < p.TotalPaginas)
            links["next"] = new() { Href = $"{basePath}?page={p.PaginaActual + 1}&limit={p.ElementosPorPagina}" };
        return links;
    }
}

public sealed class ExtrasListaPayload
{
    public IReadOnlyList<ExtraBookingResponse> Extras { get; set; } = Array.Empty<ExtraBookingResponse>();
    public PaginacionResponse Paginacion { get; set; } = new();

    [JsonPropertyName("_links")]
    public Dictionary<string, LinkHref> _Links { get; set; } = new();
}
