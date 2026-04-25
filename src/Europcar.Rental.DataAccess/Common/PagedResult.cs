namespace Europcar.Rental.DataAccess.Common;

/// <summary>
/// Resultado paginado genérico para consultas con paginación.
/// Compatible con frontend e-commerce (total pages, has next/prev, etc).
/// </summary>
public class PagedResult<T>
{
    /// <summary>Elementos de la página actual.</summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>Página actual (1-indexed).</summary>
    public int PageNumber { get; set; }

    /// <summary>Tamaño de página.</summary>
    public int PageSize { get; set; }

    /// <summary>Total de registros que cumplen el filtro.</summary>
    public int TotalCount { get; set; }

    /// <summary>Total de páginas disponibles.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>¿Hay página anterior?</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>¿Hay página siguiente?</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>Índice del primer elemento visible (1-indexed, para UI).</summary>
    public int FirstItemIndex => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;

    /// <summary>Índice del último elemento visible (para UI).</summary>
    public int LastItemIndex => Math.Min(PageNumber * PageSize, TotalCount);

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Crea un PagedResult vacío.
    /// </summary>
    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10) => new()
    {
        Items = Array.Empty<T>(),
        TotalCount = 0,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}

/// <summary>
/// Parámetros de paginación reutilizables en cualquier request.
/// </summary>
public class PaginationParams
{
    private int _pageNumber = 1;
    private int _pageSize = 10;
    private const int MaxPageSize = 50;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
    }

    /// <summary>Campo por el cual ordenar (ej: "fechaRegistro", "nombre").</summary>
    public string? SortBy { get; set; }

    /// <summary>Dirección de orden: "asc" o "desc".</summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>Término de búsqueda general.</summary>
    public string? Search { get; set; }

    public bool IsDescending => SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
}
