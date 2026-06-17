import api from './axiosClient';

/** API admin de reservas (back-office). */
export const reservasApi = {
  create: (data: Record<string, unknown>) => api.post('/admin/Reservas', data),
  update: (id: number | string, data: Record<string, unknown>) => api.put(`/admin/Reservas/${id}`, data),
  getByCliente: (idCliente: number) => api.get(`/admin/Reservas/cliente/${idCliente}`),
  getByCodigo: (codigo: string) => api.get(`/admin/Reservas/${codigo}`),
  confirmar: (id: number | string, data: Record<string, unknown> = {}) =>
    api.put(`/admin/Reservas/${id}/confirmar`, data),
  cancelar: (idReserva: number, motivo: string) =>
    api.put(`/admin/Reservas/${idReserva}/cancelar`, { motivo }),
  guestClient: (data: Record<string, unknown>) => api.post('/reservas/guest-client', data),
};

