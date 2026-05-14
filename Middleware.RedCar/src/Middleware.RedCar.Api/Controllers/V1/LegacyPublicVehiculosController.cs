using System.Globalization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.RedCar.Api.Compatibility;
using Middleware.RedCar.Business.DTOs.Booking;
using Middleware.RedCar.Business.Interfaces;

namespace Middleware.RedCar.Api.Controllers.V1;

/// <summary>
/// Rutas públicas /api/v1/vehiculos compatibles con <c>frontend/src/api/bookingApi.js</c>
/// y el monolito <c>BookingVehiculosController</c>.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/vehiculos")]
[Produces("application/json")]
public sealed class LegacyPublicVehiculosController : ControllerBase
{
    private readonly IMarketplaceOrchestrator _marketplace;
    private readonly IReservaOrchestrator _reservas;

    public LegacyPublicVehiculosController(IMarketplaceOrchestrator marketplace, IReservaOrchestrator reservas)
    {
        _marketplace = marketplace;
        _reservas = reservas;
    }

    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] VehiculoFiltroRequest filtro, CancellationToken ct)
    {
        var (items, paginacion) = await _marketplace.BuscarVehiculosAsync(filtro, ct);
        var flat = items.Select(LegacyV1DtoMapper.ToVehiculoListaItem).ToList();
        var data = new
        {
            vehiculos = flat,
            paginacion,
            _links = BuildVehiculosListLinks(paginacion, filtro)
        };
        return Ok(LegacyBookingEnvelope.Ok(data));
    }

    [HttpGet("{vehiculoId}")]
    public async Task<IActionResult> Detalle([FromRoute] string vehiculoId, CancellationToken ct)
    {
        if (!int.TryParse(vehiculoId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            return NotFound(LegacyBookingEnvelope.Fail("Vehículo no encontrado o id no numérico.", 404));

        var detalle = await _marketplace.GetVehiculoDetalleAsync(id, ct);
        return Ok(LegacyBookingEnvelope.Ok(new { vehiculo = LegacyV1DtoMapper.ToVehiculoDetalle(detalle) }));
    }

    [HttpGet("{vehiculoId}/disponibilidad")]
    public async Task<IActionResult> Disponibilidad(
        [FromRoute] string vehiculoId,
        [FromQuery] DateTimeOffset fechaRecogida,
        [FromQuery] DateTimeOffset fechaDevolucion,
        [FromQuery] int idLocalizacion,
        CancellationToken ct)
    {
        if (!int.TryParse(vehiculoId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idVehiculo))
            return BadRequest(LegacyBookingEnvelope.Fail("idVehiculo debe ser numérico.", 400));

        var dto = await _reservas.VerificarDisponibilidadAsync(idVehiculo, idLocalizacion, fechaRecogida, fechaDevolucion, ct);
        return Ok(LegacyBookingEnvelope.Ok(dto));
    }

    private static Dictionary<string, LinkHref> BuildVehiculosListLinks(PaginacionResponse p, VehiculoFiltroRequest filtro)
    {
        var basePath = "/api/v1/vehiculos";
        var qs = BuildVehiculosQueryString(filtro, p.PaginaActual, p.ElementosPorPagina);
        var links = new Dictionary<string, LinkHref>
        {
            ["self"] = new() { Href = $"{basePath}?{qs}" }
        };
        if (p.PaginaActual < p.TotalPaginas)
            links["next"] = new() { Href = $"{basePath}?{BuildVehiculosQueryString(filtro, p.PaginaActual + 1, p.ElementosPorPagina)}" };
        if (p.PaginaActual > 1)
            links["prev"] = new() { Href = $"{basePath}?{BuildVehiculosQueryString(filtro, p.PaginaActual - 1, p.ElementosPorPagina)}" };
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
