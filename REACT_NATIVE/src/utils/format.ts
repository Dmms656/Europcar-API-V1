export function formatCurrency(value: number | undefined | null) {
  const n = Number(value ?? 0);
  return `$${n.toFixed(2)}`;
}

export function formatDateEs(value: string | undefined | null, withTime = false) {
  if (!value) return '—';
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return '—';
  return d.toLocaleDateString('es-EC', withTime ? { dateStyle: 'medium', timeStyle: 'short' } : undefined);
}

export function contratoCodigo(c: { codigoContrato?: string; numero?: string; idContrato?: number; id?: number }) {
  return c.codigoContrato || c.numero || `CTR-${c.idContrato ?? c.id ?? '?'}`;
}

export function contratoEstado(c: { estadoContrato?: string; estado?: string }) {
  return c.estadoContrato || c.estado || '—';
}
