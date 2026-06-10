using System.Globalization;
using RedCar.Integration.GraphQl.Graph.Types;
using RedCar.Integration.GraphQl.Services;

namespace RedCar.Integration.GraphQl.Graph;

public sealed class BookingQuery
{
    public async Task<VehiculoPagedGql?> Vehiculos(
        VehiculoFiltroInput filtro,
        [Service] MsHttpGateway gateway,
        CancellationToken ct)
    {
        var qs = new List<string>
        {
            $"idLocalizacion={filtro.IdLocalizacion}",
            $"fechaRecogida={Uri.EscapeDataString(filtro.FechaRecogida.ToString("O"))}",
            $"fechaDevolucion={Uri.EscapeDataString(filtro.FechaDevolucion.ToString("O"))}",
            $"page={filtro.Page}",
            $"limit={filtro.Limit}"
        };
        if (!string.IsNullOrWhiteSpace(filtro.NombreCategoria)) qs.Add($"nombreCategoria={Uri.EscapeDataString(filtro.NombreCategoria)}");
        if (!string.IsNullOrWhiteSpace(filtro.NombreMarca)) qs.Add($"nombreMarca={Uri.EscapeDataString(filtro.NombreMarca)}");
        if (!string.IsNullOrWhiteSpace(filtro.Transmision)) qs.Add($"transmision={Uri.EscapeDataString(filtro.Transmision)}");
        if (!string.IsNullOrWhiteSpace(filtro.Sort)) qs.Add($"sort={Uri.EscapeDataString(filtro.Sort)}");

        var paged = await gateway.GetCatalogoAsync<PagedDto<VehiculoDto>>($"/api/v1/vehiculos?{string.Join('&', qs)}", ct);
        if (paged is null) return new VehiculoPagedGql();

        var items = new List<VehiculoGql>(paged.Items.Count);
        foreach (var v in paged.Items)
        {
            var disp = await gateway.GetReservasAsync<DisponibilidadDto>(
                $"/api/v1/reservas/disponibilidad?idVehiculo={v.IdVehiculo}&idLocalizacion={filtro.IdLocalizacion}" +
                $"&fechaRecogida={Uri.EscapeDataString(filtro.FechaRecogida.ToString("O"))}" +
                $"&fechaDevolucion={Uri.EscapeDataString(filtro.FechaDevolucion.ToString("O"))}", ct);

            items.Add(new VehiculoGql
            {
                IdVehiculo = v.IdVehiculo,
                CodigoInterno = v.CodigoInterno ?? string.Empty,
                Marca = v.Marca ?? string.Empty,
                Modelo = v.Modelo ?? string.Empty,
                IdLocalizacion = v.IdLocalizacion,
                Estado = v.Estado ?? string.Empty,
                PrecioDia = v.PrecioDia,
                NombreCategoria = v.NombreCategoria,
                Transmision = v.Transmision,
                Disponible = disp?.Disponible
            });
        }

        return new VehiculoPagedGql
        {
            Items = items,
            TotalElementos = paged.TotalElementos,
            PaginaActual = filtro.Page,
            ElementosPorPagina = filtro.Limit
        };
    }

    public Task<VehiculoGql?> Vehiculo(int id, [Service] MsHttpGateway gateway, CancellationToken ct) =>
        gateway.GetCatalogoAsync<VehiculoGql>($"/api/v1/vehiculos/{id}", ct);

    public Task<LocalizacionGql?> Localizacion(int id, [Service] MsHttpGateway gateway, CancellationToken ct) =>
        gateway.GetLocalizacionesAsync<LocalizacionGql>($"/api/v1/localizaciones/{id}", ct);

    public async Task<LocalizacionPagedGql?> Localizaciones(
        int? idCiudad,
        int page,
        int limit,
        [Service] MsHttpGateway gateway,
        CancellationToken ct)
    {
        var qs = $"page={page}&limit={limit}";
        if (idCiudad is > 0) qs += $"&idCiudad={idCiudad.Value}";
        var paged = await gateway.GetLocalizacionesAsync<PagedDto<LocalizacionGql>>($"/api/v1/localizaciones?{qs}", ct);
        return paged is null
            ? new LocalizacionPagedGql()
            : new LocalizacionPagedGql { Items = paged.Items, TotalElementos = paged.TotalElementos };
    }

    public async Task<DisponibilidadGql?> Disponibilidad(
        DisponibilidadInput input,
        [Service] MsHttpGateway gateway,
        CancellationToken ct)
    {
        var path = $"/api/v1/reservas/disponibilidad?idVehiculo={input.IdVehiculo}&idLocalizacion={input.IdLocalizacion}" +
                   $"&fechaRecogida={Uri.EscapeDataString(input.FechaRecogida.ToString("O"))}" +
                   $"&fechaDevolucion={Uri.EscapeDataString(input.FechaDevolucion.ToString("O"))}";
        var dto = await gateway.GetReservasAsync<DisponibilidadDto>(path, ct);
        return dto is null
            ? null
            : new DisponibilidadGql
            {
                IdVehiculo = input.IdVehiculo,
                IdLocalizacion = input.IdLocalizacion,
                Disponible = dto.Disponible
            };
    }

    public async Task<CategoriaPagedGql?> Categorias(
        int page,
        int limit,
        [Service] MsHttpGateway gateway,
        CancellationToken ct)
    {
        var paged = await gateway.GetCatalogoAsync<PagedDto<CategoriaGql>>($"/api/v1/categorias?page={page}&limit={limit}", ct);
        return paged is null
            ? new CategoriaPagedGql()
            : new CategoriaPagedGql { Items = paged.Items, TotalElementos = paged.TotalElementos };
    }

    public async Task<ExtraPagedGql?> Extras(
        int page,
        int limit,
        [Service] MsHttpGateway gateway,
        CancellationToken ct)
    {
        var paged = await gateway.GetCatalogoAsync<PagedDto<ExtraGql>>($"/api/v1/extras?page={page}&limit={limit}", ct);
        return paged is null
            ? new ExtraPagedGql()
            : new ExtraPagedGql { Items = paged.Items, TotalElementos = paged.TotalElementos };
    }

    public Task<ReservaGql?> Reserva(string codigo, [Service] MsHttpGateway gateway, CancellationToken ct) =>
        gateway.GetReservasAsync<ReservaGql>($"/api/v1/reservas/{Uri.EscapeDataString(codigo)}", ct);

    public Task<FacturaGql?> Factura(string codigoReserva, [Service] MsHttpGateway gateway, CancellationToken ct) =>
        gateway.GetReservasAsync<FacturaGql>($"/api/v1/reservas/{Uri.EscapeDataString(codigoReserva)}/factura", ct);

    private sealed class PagedDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalElementos { get; set; }
    }

    private sealed class VehiculoDto
    {
        public int IdVehiculo { get; set; }
        public string? CodigoInterno { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public int IdLocalizacion { get; set; }
        public string? Estado { get; set; }
        public decimal PrecioDia { get; set; }
        public string? NombreCategoria { get; set; }
        public string? Transmision { get; set; }
    }

    private sealed class DisponibilidadDto
    {
        public bool Disponible { get; set; }
    }
}
