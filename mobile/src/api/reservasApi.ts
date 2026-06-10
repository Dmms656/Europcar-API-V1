import api from './axiosClient';

/** Misma API admin que usa el portal cliente web (requiere JWT). */
export const reservasApi = {
  getByCliente: (idCliente: number) => api.get(`/admin/Reservas/cliente/${idCliente}`),
  getByCodigo: (codigo: string) => api.get(`/admin/Reservas/${codigo}`),
  cancelar: (idReserva: number, motivo: string) =>
    api.put(`/admin/Reservas/${idReserva}/cancelar`, { motivo }),
};
