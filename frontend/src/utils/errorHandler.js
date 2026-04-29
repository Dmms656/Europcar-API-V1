/**
 * Centraliza la conversión de errores de API/red en mensajes legibles
 * y estructurados para la UI. La API responde siempre con shape:
 *   { success, message, data, statusCode, errors?, traceId?, detail? }
 *
 * Este util:
 *  - Extrae el mensaje principal.
 *  - Aplana errors por campo a una lista plana.
 *  - Detecta errores de red (sin response) y timeouts.
 *  - Normaliza el resultado a { status, message, fieldErrors, traceId, isNetwork, isTimeout, original }.
 */

const DEFAULT_MESSAGES = {
  0: 'No hay conexión con el servidor. Verifica tu internet.',
  400: 'La petición no es válida.',
  401: 'Tu sesión expiró. Inicia sesión nuevamente.',
  403: 'No tienes permisos para realizar esta acción.',
  404: 'El recurso solicitado no existe.',
  408: 'La petición tardó demasiado. Intenta nuevamente.',
  409: 'Conflicto: el recurso fue modificado o ya existe.',
  413: 'El contenido enviado es demasiado grande.',
  422: 'Algunos campos no son válidos.',
  429: 'Demasiadas peticiones. Espera unos segundos.',
  500: 'Error interno del servidor. Intenta más tarde.',
  502: 'Servicio temporalmente no disponible.',
  503: 'El servidor está sobrecargado o en mantenimiento.',
  504: 'El servidor tardó demasiado en responder.',
};

/**
 * Devuelve un objeto normalizado con la información del error.
 * Nunca lanza; siempre devuelve un objeto.
 */
export function parseApiError(error) {
  // Caso 1: el error ya es un objeto plano (puede venir de un throw manual)
  if (!error) {
    return baseResult({ status: 0, message: 'Error desconocido' });
  }

  // Caso 2: AbortError / cancelado
  if (error.name === 'CanceledError' || error.code === 'ERR_CANCELED') {
    return baseResult({ status: 0, message: 'Operación cancelada', isCanceled: true, original: error });
  }

  // Caso 3: Timeout de Axios
  if (error.code === 'ECONNABORTED') {
    return baseResult({
      status: 408,
      message: DEFAULT_MESSAGES[408],
      isTimeout: true,
      original: error,
    });
  }

  // Caso 4: Sin response (error de red, CORS, DNS, servidor caído)
  if (!error.response) {
    return baseResult({
      status: 0,
      message: DEFAULT_MESSAGES[0],
      isNetwork: true,
      original: error,
    });
  }

  const { status, data } = error.response;
  const apiPayload = data && typeof data === 'object' ? data : {};

  // El backend envía .message; si no, usamos defaults o el statusText
  const message =
    apiPayload.message ||
    apiPayload.title ||
    DEFAULT_MESSAGES[status] ||
    error.message ||
    'Ha ocurrido un error';

  const fieldErrors = flattenFieldErrors(apiPayload.errors);

  return baseResult({
    status,
    message: sanitizeUserMessage(message),
    fieldErrors,
    traceId: apiPayload.traceId || null,
    detail: apiPayload.detail || null,
    original: error,
  });
}

/**
 * Devuelve el mensaje plano y "humano" para mostrar en un toast.
 */
export function getErrorMessage(error) {
  return parseApiError(error).message;
}

/**
 * Devuelve un dict { campo: 'mensaje' } útil para react-hook-form.setError o
 * inputs controlados con errores por campo.
 */
export function getFieldErrors(error) {
  return parseApiError(error).fieldErrors;
}

// ----------------------------------------------------------------------------
// Helpers privados
// ----------------------------------------------------------------------------

function baseResult(partial) {
  return {
    status: 0,
    message: '',
    fieldErrors: {},
    traceId: null,
    detail: null,
    isNetwork: false,
    isTimeout: false,
    isCanceled: false,
    original: null,
    ...partial,
  };
}

function flattenFieldErrors(errors) {
  if (!errors || typeof errors !== 'object') return {};
  const out = {};
  for (const [field, messages] of Object.entries(errors)) {
    if (Array.isArray(messages)) {
      out[field] = sanitizeUserMessage(messages[0] || '');
    } else if (typeof messages === 'string') {
      out[field] = sanitizeUserMessage(messages);
    }
  }
  return out;
}

function sanitizeUserMessage(rawMessage) {
  const text = String(rawMessage || '');
  return text
    .replace(/\s*ref:\s*[A-Z0-9:-]+/gi, '')
    .replace(/\s*traceid:\s*[A-Z0-9:-]+/gi, '')
    .replace(/\n{2,}/g, '\n')
    .trim();
}
