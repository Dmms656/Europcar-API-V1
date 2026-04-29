import axios from 'axios';
import { toast } from 'sonner';
import { parseApiError } from '../utils/errorHandler';

/**
 * Cliente HTTP con manejo de errores centralizado.
 *
 *  - Inyecta el JWT en cada request.
 *  - Reintenta automáticamente en errores transitorios de red / 5xx (excepto 501).
 *  - Maneja 401 globalmente: limpia sesión y, si la ruta actual es protegida,
 *    redirige a /login conservando "from".
 *  - Nunca redirige desde rutas públicas (catálogo, home, login, registro, etc.).
 *  - Permite suprimir el toast automático con { suppressErrorToast: true } en la config.
 */

const PUBLIC_PATHS = ['/', '/buscar', '/catalogo', '/login', '/registro', '/reservar'];
const MAX_RETRIES = 3;
const RETRY_BACKOFF_MS = 800;

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 45000,
});

// === Request interceptor: token + tracking de retries ===
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  if (config.__retryCount == null) config.__retryCount = 0;
  return config;
});

// === Response interceptor: errores ===
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const config = error.config || {};
    const status = error.response?.status;

    // 1) Reintento de transitorios (red sin respuesta o 502/503/504)
    if (shouldRetry(error) && config.__retryCount < MAX_RETRIES) {
      config.__retryCount += 1;
      const delay = RETRY_BACKOFF_MS * 2 ** (config.__retryCount - 1);
      await wait(delay);
      return api(config);
    }

    // 2) 401: sesión inválida
    if (status === 401) {
      handleUnauthorized();
    }

    // 3) Toast automático (a menos que se suprima)
    if (!config.suppressErrorToast) {
      const parsed = parseApiError(error);
      // No spammear el toast en 401 (ya redirigimos) ni en 422 (suelen ser errores de campo gestionados por la pantalla)
      if (status !== 401 && status !== 422 && !parsed.isCanceled) {
        toast.error(parsed.message, {
          duration: parsed.isNetwork ? 6000 : 4000,
        });
      }
    }

    return Promise.reject(error);
  }
);

// ----------------------------------------------------------------------------
// Helpers privados
// ----------------------------------------------------------------------------

function shouldRetry(error) {
  if (error.code === 'ERR_CANCELED') return false;
  // Métodos no idempotentes no se reintentan automáticamente
  const method = (error.config?.method || 'get').toLowerCase();
  if (!['get', 'head', 'options'].includes(method)) return false;

  // Sin respuesta (red): reintentar
  if (!error.response) return true;
  // Status transitorios
  return [408, 429, 502, 503, 504].includes(error.response.status);
}

function wait(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function handleUnauthorized() {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  localStorage.removeItem('userType');

  const path = window.location.pathname;
  const isPublic = PUBLIC_PATHS.some((p) =>
    p === '/' ? path === '/' : path === p || path.startsWith(`${p}/`)
  );
  if (isPublic) return;

  // Evitar bucle si ya estamos navegando al login
  if (path === '/login') return;

  const from = `${path}${window.location.search}`;
  window.location.href = `/login?from=${encodeURIComponent(from)}`;
}

export default api;
