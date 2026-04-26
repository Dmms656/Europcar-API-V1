import api from './axiosClient';

export const pagosApi = {
  getById: (id) => api.get(`/Pagos/${id}`),
  getByReserva: (idReserva) => api.get(`/Pagos/reserva/${idReserva}`),
  create: (data) => api.post('/Pagos', data),
};
