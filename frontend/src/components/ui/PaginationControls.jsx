export default function PaginationControls({
  page,
  totalPages,
  pageSize,
  onPageChange,
  onPageSizeChange,
  totalItems,
  startItem,
  endItem,
}) {
  if (totalItems <= 0) return null;

  return (
    <div className="module-page__toolbar" style={{ marginTop: 12 }}>
      <div style={{ color: 'var(--color-text-secondary)', fontSize: '0.9rem' }}>
        Mostrando {startItem}-{endItem} de {totalItems}
      </div>
      <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
        <label style={{ fontSize: '0.9rem', color: 'var(--color-text-secondary)' }}>
          Por página:
        </label>
        <select
          className="form-input"
          style={{ maxWidth: 90 }}
          value={pageSize}
          onChange={(e) => onPageSizeChange(Number(e.target.value))}
        >
          <option value={10}>10</option>
          <option value={20}>20</option>
          <option value={50}>50</option>
          <option value={100}>100</option>
        </select>
        <button
          className="btn btn--outline btn--sm"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          Anterior
        </button>
        <span style={{ minWidth: 90, textAlign: 'center', fontSize: '0.9rem' }}>
          {page} / {totalPages}
        </span>
        <button
          className="btn btn--outline btn--sm"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Siguiente
        </button>
      </div>
    </div>
  );
}
