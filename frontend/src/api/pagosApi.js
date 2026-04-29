import api from './axiosClient';

export const pagosApi = {
  getAll: () => api.get('/Pagos'),
  getById: (id) => api.get(`/Pagos/${id}`),
  getByReserva: (idReserva) => api.get(`/Pagos/reserva/${idReserva}`),
  create: (data) => api.post('/Pagos', data),
  update: (id, data) => api.put(`/Pagos/${id}`, data),
};
