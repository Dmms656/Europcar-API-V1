import axios from 'axios';
import { toast } from 'sonner';
import { resolveApiBaseUrl, isApiConfigured } from '../config/api';
import { useAuthStore } from '../store/useAuthStore';
import { parseApiError } from '../utils/errorHandler';

/**
 * Cliente HTTP con manejo de errores centralizado.
 * Autenticación: cookie HttpOnly rc_auth + withCredentials.
 * No se guarda el JWT en localStorage (laboratorio de secretos / XSS).
 */

const PUBLIC_PATHS = ['/', '/buscar', '/catalogo', '/login', '/registro', '/reservar'];
const MAX_RETRIES = 3;
const RETRY_BACKOFF_MS = 800;

const api = axios.create({
  baseURL: resolveApiBaseUrl(),
  headers: { 'Content-Type': 'application/json' },
  timeout: 45000,
  withCredentials: true,
});

api.interceptors.request.use((config) => {
  if (!isApiConfigured()) {
    return Promise.reject(
      Object.assign(new Error('VITE_API_URL no configurada'), {
        code: 'ERR_API_NOT_CONFIGURED',
      })
    );
  }
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  if (config.__retryCount == null) config.__retryCount = 0;
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const config = error.config || {};
    const status = error.response?.status;
    const method = (config.method || 'get').toLowerCase();
    const isReadRequest = ['get', 'head', 'options'].includes(method);

    if (shouldRetry(error) && config.__retryCount < MAX_RETRIES) {
      config.__retryCount += 1;
      const delay = RETRY_BACKOFF_MS * 2 ** (config.__retryCount - 1);
      await wait(delay);
      return api(config);
    }

    if (status === 401 && !config.suppressAuthRedirect) {
      handleUnauthorized();
    }

    if (!config.suppressErrorToast) {
      const parsed = parseApiError(error);
      const skipAutoToast = isReadRequest && (!status || status >= 500 || parsed.isNetwork || parsed.isTimeout);
      if (status !== 401 && status !== 422 && !parsed.isCanceled && !skipAutoToast) {
        toast.error(parsed.message, {
          duration: parsed.isNetwork ? 6000 : 4000,
        });
      }
    }

    return Promise.reject(error);
  }
);

function shouldRetry(error) {
  if (error.code === 'ERR_CANCELED' || error.code === 'ERR_API_NOT_CONFIGURED') return false;
  const method = (error.config?.method || 'get').toLowerCase();
  if (!['get', 'head', 'options'].includes(method)) return false;
  if (!error.response) return true;
  return [408, 429, 502, 503, 504].includes(error.response.status);
}

function wait(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function handleUnauthorized() {
  useAuthStore.getState().clearAuth();

  const path = window.location.pathname;
  const isPublic = PUBLIC_PATHS.some((p) =>
    p === '/' ? path === '/' : path === p || path.startsWith(`${p}/`)
  );
  if (isPublic) return;
  if (path === '/login') return;

  const from = `${path}${window.location.search}`;
  window.location.href = `/login?from=${encodeURIComponent(from)}`;
}

export default api;
