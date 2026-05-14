using Europcar.Rental.DataAccess.Entities.Rental;

namespace Europcar.Rental.DataAccess.Queries;

/// <summary>
/// Extensiones de IQueryable para consultas complejas de reservas.
/// </summary>
public static class ReservaQueryExtensions
{
    public static IQueryable<ReservaEntity> FiltrarActivas(this IQueryable<ReservaEntity> query)
    {
        return query.Where(r => r.EstadoReserva != "CANCELADA" && r.EstadoReserva != "NO_SHOW");
    }

    public static IQueryable<ReservaEntity> FiltrarPorEstado(this IQueryable<ReservaEntity> query, string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado)) return query;
        return query.Where(r => r.EstadoReserva == estado.ToUpper());
    }

    public static IQueryable<ReservaEntity> FiltrarPorCliente(this IQueryable<ReservaEntity> query, int? clienteId)
    {
        if (!clienteId.HasValue) return query;
        return query.Where(r => r.IdCliente == clienteId.Value);
    }

    public static IQueryable<ReservaEntity> FiltrarPorRangoFechas(
        this IQueryable<ReservaEntity> query,
        DateTimeOffset? fechaDesde,
        DateTimeOffset? fechaHasta)
    {
        if (fechaDesde.HasValue)
            query = query.Where(r => r.FechaHoraRecogida >= fechaDesde.Value);
        if (fechaHasta.HasValue)
            query = query.Where(r => r.FechaHoraDevolucion <= fechaHasta.Value);
        return query;
    }

    /// <summary>
    /// Detecta si un vehículo tiene reservas que se solapan con un rango de fechas dado.
    /// </summary>
    public static IQueryable<ReservaEntity> SolapadasConVehiculo(
        this IQueryable<ReservaEntity> query,
        int idVehiculo,
        DateTimeOffset fechaInicio,
        DateTimeOffset fechaFin,
        int? idReservaExcluir = null)
    {
        var q = query.Where(r =>
            r.IdVehiculo == idVehiculo
            && r.EstadoReserva != "CANCELADA"
            && r.EstadoReserva != "FINALIZADA"
            && r.EstadoReserva != "NO_SHOW"
            && r.FechaHoraRecogida < fechaFin
            && r.FechaHoraDevolucion > fechaInicio);

        if (idReservaExcluir.HasValue)
            q = q.Where(r => r.IdReserva != idReservaExcluir.Value);

        return q;
    }
}
