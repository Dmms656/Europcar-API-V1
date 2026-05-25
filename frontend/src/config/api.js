/**
 * URL base de la API (incluye /api/v1).
 * - Con proxy de Vite (dev): dejar vacío o usar /api/v1 relativo.
 * - Directo a middleware local: http://localhost:5200/api/v1
 * - Producción Render: https://europcar-api-v2.onrender.com/api/v1
 */
export function resolveApiBaseUrl() {
  const fromEnv = import.meta.env.VITE_API_URL?.trim();
  if (fromEnv) return fromEnv.replace(/\/$/, '');

  if (import.meta.env.DEV) {
    return '/api/v1';
  }

  return '';
}

export function isApiConfigured() {
  return Boolean(resolveApiBaseUrl());
}
