namespace RedCar.Shared.Contracts.Common;

/// <summary>
/// Resultado paginado estandar para endpoints de listado.
/// </summary>
public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required long Total { get; init; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)Total / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;
}
