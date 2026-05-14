using System.Globalization;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Models.Common;
using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.Business.Interfaces;

namespace Middleware.RedCar.Api.Controllers.V2.Booking;

/// <summary>
/// Endpoints 1 y 2 del contrato (busqueda y detalle de vehiculos).
/// El endpoint 3 (disponibilidad) vive en ReservasBookingController porque
/// la URL del contrato es /api/v2/booking/reservas/{idVehiculo}/disponibilidad.
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/booking/vehiculos")]
[Produces("application/json")]
public sealed class VehiculosBookingController : ControllerBase
{
    private readonly IMarketplaceOrchestrator _marketplace;

    public VehiculosBookingController(IMarketplaceOrchestrator marketplace)
    {
        _marketplace = marketplace;
    }

    /// <summary>
    /// Endpoint 1 - Busqueda de vehiculos paginada.
    /// GET /api/v2/booking/vehiculos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<VehiculosBusquedaPayload>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<ActionResult<ApiResponse<VehiculosBusquedaPayload>>> Buscar(
        [FromQuery] VehiculoFiltroRequest filtro,
        CancellationToken ct)
    {
        var (items, paginacion) = await _marketplace.BuscarVehiculosAsync(filtro, ct);

        var payload = new VehiculosBusquedaPayload
        {
            Vehiculos = items,
            Paginacion = paginacion,
            _Links = BuildPaginationLinks("/api/v2/booking/vehiculos", paginacion, filtro)
        };

        return Ok(ApiResponse<VehiculosBusquedaPayload>.Ok(payload));
    }

    /// <summary>
    /// Endpoint 2 - Detalle de un vehiculo especifico.
    /// GET /api/v2/booking/vehiculos/{idVehiculo}
    /// </summary>
    [HttpGet("{idVehiculo:int}")]
    [ProducesResponseType(typeof(ApiResponse<VehiculoDetallePayload>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<ActionResult<ApiResponse<VehiculoDetallePayload>>> Detalle(
        [FromRoute] int idVehiculo,
        CancellationToken ct)
    {
        var detalle = await _marketplace.GetVehiculoDetalleAsync(idVehiculo, ct);
        return Ok(ApiResponse<VehiculoDetallePayload>.Ok(new VehiculoDetallePayload { Vehiculo = detalle }));
    }

    private static Dictionary<string, LinkHref> BuildPaginationLinks(string basePath, PaginacionResponse p, VehiculoFiltroRequest filtro)
    {
        var qs = BuildVehiculosQueryString(filtro, p.PaginaActual, p.ElementosPorPagina);
        var links = new Dictionary<string, LinkHref>
        {
            ["self"] = new() { Href = $"{basePath}?{qs}" }
        };
        if (p.PaginaActual < p.TotalPaginas)
        {
            var qsNext = BuildVehiculosQueryString(filtro, p.PaginaActual + 1, p.ElementosPorPagina);
            links["next"] = new() { Href = $"{basePath}?{qsNext}" };
        }
        if (p.PaginaActual > 1)
        {
            var qsPrev = BuildVehiculosQueryString(filtro, p.PaginaActual - 1, p.ElementosPorPagina);
            links["prev"] = new() { Href = $"{basePath}?{qsPrev}" };
        }
        return links;
    }

    private static string BuildVehiculosQueryString(VehiculoFiltroRequest f, int page, int limit)
    {
        var parts = new List<string>
        {
            $"idLocalizacion={f.IdLocalizacion.ToString(CultureInfo.InvariantCulture)}",
            $"fechaRecogida={Uri.EscapeDataString(f.FechaRecogida.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture))}",
            $"fechaDevolucion={Uri.EscapeDataString(f.FechaDevolucion.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture))}",
            $"page={page.ToString(CultureInfo.InvariantCulture)}",
            $"limit={limit.ToString(CultureInfo.InvariantCulture)}"
        };
        if (!string.IsNullOrWhiteSpace(f.NombreCategoria))
            parts.Add($"nombreCategoria={Uri.EscapeDataString(f.NombreCategoria!)}");
        if (!string.IsNullOrWhiteSpace(f.NombreMarca))
            parts.Add($"nombreMarca={Uri.EscapeDataString(f.NombreMarca!)}");
        if (!string.IsNullOrWhiteSpace(f.Transmision))
            parts.Add($"transmision={Uri.EscapeDataString(f.Transmision!)}");
        if (!string.IsNullOrWhiteSpace(f.Sort))
            parts.Add($"sort={Uri.EscapeDataString(f.Sort!)}");
        return string.Join('&', parts);
    }
}

public sealed class VehiculosBusquedaPayload
{
    public IReadOnlyList<VehiculoBookingResponse> Vehiculos { get; set; } = Array.Empty<VehiculoBookingResponse>();
    public PaginacionResponse Paginacion { get; set; } = new();

    [JsonPropertyName("_links")]
    public Dictionary<string, LinkHref> _Links { get; set; } = new();
}

public sealed class VehiculoDetallePayload
{
    public VehiculoDetalleResponse Vehiculo { get; set; } = new();
}
