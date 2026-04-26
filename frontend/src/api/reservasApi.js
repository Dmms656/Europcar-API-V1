import api from './axiosClient';

export const reservasApi = {
  create: (data) => api.post('/Reservas', data),
  getByCodigo: (codigo) => api.get(`/Reservas/${codigo}`),
  getByCliente: (idCliente) => api.get(`/Reservas/cliente/${idCliente}`),
  confirmar: (id) => api.put(`/Reservas/${id}/confirmar`),
  cancelar: (id, motivo) => api.put(`/Reservas/${id}/cancelar`, { motivo }),
};
