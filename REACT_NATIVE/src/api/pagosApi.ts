import api from './axiosClient';

export const pagosApi = {
  getAll: () => api.get('/Pagos'),
  getById: (id: number | string) => api.get(`/Pagos/${id}`),
  getByReserva: (idReserva: number | string) => api.get(`/Pagos/reserva/${idReserva}`),
  create: (data: Record<string, unknown>) => api.post('/Pagos', data),
  update: (id: number | string, data: Record<string, unknown>) => api.put(`/Pagos/${id}`, data),
};
