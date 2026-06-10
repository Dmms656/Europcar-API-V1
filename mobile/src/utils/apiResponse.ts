export function unwrapData<T>(response: { data?: unknown }): T | null {
  const body = response.data as { data?: T; Data?: T } | undefined;
  return body?.data ?? body?.Data ?? null;
}

export function getErrorMessage(error: unknown): string {
  if (!error || typeof error !== 'object') return 'Error desconocido';
  const err = error as { response?: { data?: { message?: string; mensaje?: string } }; message?: string };
  return (
    err.response?.data?.message ||
    err.response?.data?.mensaje ||
    err.message ||
    'No se pudo completar la operación'
  );
}
