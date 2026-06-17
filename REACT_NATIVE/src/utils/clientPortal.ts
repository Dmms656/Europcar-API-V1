/** Rutas del portal cliente (mismo conjunto que frontend/ ClienteLayout). */
export const CLIENT_PORTAL_SEGMENTS = [
  'mi-cuenta',
  'mis-reservas',
  'mis-contratos',
  'mis-facturas',
  'historial',
] as const;

export function isClientPortalPath(pathname: string): boolean {
  return CLIENT_PORTAL_SEGMENTS.some((segment) => pathname.includes(segment));
}
