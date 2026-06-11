import api from './axiosClient';
import { unwrapData } from '@/src/utils/apiResponse';

export const adminVehiculosApi = {
  getAll: () => api.get('/admin/Vehiculos'),
  getDisponibles: () => api.get('/admin/Vehiculos/disponibles'),
  getById: (id: number | string) => api.get(`/admin/Vehiculos/${id}`),
};

export const adminClientesApi = {
  getAll: (params?: Record<string, unknown>) => api.get('/admin/Clientes', { params }),
  getById: (id: number | string) => api.get(`/admin/Clientes/${id}`),
};

export const adminReservasApi = {
  getByCodigo: (codigo: string) => api.get(`/admin/Reservas/${codigo}`),
  getByCliente: (idCliente: number | string) => api.get(`/admin/Reservas/cliente/${idCliente}`),
};

type ClienteRow = { idCliente?: number };
type ReservaRow = {
  idReserva?: number;
  codigoReserva?: string;
  estadoReserva?: string;
  fechaInicio?: string;
  fechaFin?: string;
  total?: number;
  idCliente?: number;
};

/** No existe GET /admin/Reservas — agrega por cliente como en la web. */
export async function listAdminReservas(): Promise<ReservaRow[]> {
  const clientesRes = await adminClientesApi.getAll();
  const clientes = unwrapData<ClienteRow[]>(clientesRes) ?? [];
  if (!clientes.length) return [];

  const responses = await Promise.allSettled(
    clientes
      .filter((c) => c.idCliente != null)
      .map((c) => adminReservasApi.getByCliente(c.idCliente!)),
  );

  const all: ReservaRow[] = [];
  const seen = new Set<number>();
  responses.forEach((res) => {
    if (res.status !== 'fulfilled') return;
    const items = unwrapData<ReservaRow[]>(res.value) ?? [];
    items.forEach((r) => {
      const id = r.idReserva ?? 0;
      if (id && seen.has(id)) return;
      if (id) seen.add(id);
      all.push(r);
    });
  });
  return all;
}

export async function loadAdminVehiculosWithRetry(retries = 2) {
  let lastError: unknown;
  for (let i = 0; i <= retries; i++) {
    try {
      const res = await adminVehiculosApi.getAll();
      return unwrapData<Record<string, unknown>[]>(res) ?? [];
    } catch (e) {
      lastError = e;
      if (i < retries) await new Promise((r) => setTimeout(r, 1500 * (i + 1)));
    }
  }
  throw lastError;
}
