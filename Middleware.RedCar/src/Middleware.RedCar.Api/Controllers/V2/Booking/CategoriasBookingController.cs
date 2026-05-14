using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Models.Common;
using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.Business.Interfaces;

namespace Middleware.RedCar.Api.Controllers.V2.Booking;

/// <summary>
/// Endpoint 6 del contrato - Listar categorias de vehiculos.
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/booking/categorias")]
[Produces("application/json")]
public sealed class CategoriasBookingController : ControllerBase
{
    private readonly IMarketplaceOrchestrator _marketplace;

    public CategoriasBookingController(IMarketplaceOrchestrator marketplace)
    {
        _marketplace = marketplace;
    }

    /// <summary>
    /// GET /api/v2/booking/categorias
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CategoriasListaPayload>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<ActionResult<ApiResponse<CategoriasListaPayload>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var (items, paginacion) = await _marketplace.ListCategoriasAsync(page, limit, ct);

        var payload = new CategoriasListaPayload
        {
            Categorias = items,
            Paginacion = paginacion,
            _Links = BuildLinks(paginacion)
        };
        return Ok(ApiResponse<CategoriasListaPayload>.Ok(payload));
    }

    private static Dictionary<string, LinkHref> BuildLinks(PaginacionResponse p)
    {
        var basePath = "/api/v2/booking/categorias";
        var links = new Dictionary<string, LinkHref>
        {
            ["self"] = new() { Href = $"{basePath}?page={p.PaginaActual}&limit={p.ElementosPorPagina}" }
        };
        if (p.PaginaActual < p.TotalPaginas)
            links["next"] = new() { Href = $"{basePath}?page={p.PaginaActual + 1}&limit={p.ElementosPorPagina}" };
        return links;
    }
}

public sealed class CategoriasListaPayload
{
    public IReadOnlyList<CategoriaBookingResponse> Categorias { get; set; } = Array.Empty<CategoriaBookingResponse>();
    public PaginacionResponse Paginacion { get; set; } = new();

    [JsonPropertyName("_links")]
    public Dictionary<string, LinkHref> _Links { get; set; } = new();
}
