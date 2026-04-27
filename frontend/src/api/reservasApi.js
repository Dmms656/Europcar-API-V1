import api from './axiosClient';

export const reservasApi = {
  create: (data) => api.post('/Reservas', data),
  getByCodigo: (codigo) => api.get(`/Reservas/${codigo}`),
  getByCliente: (idCliente) => api.get(`/Reservas/cliente/${idCliente}`),
  confirmar: (id, data) => api.put(`/Reservas/${id}/confirmar`, data || {}),
  cancelar: (id, motivo) => api.put(`/Reservas/${id}/cancelar`, { motivo }),
  guestClient: (data) => api.post('/Reservas/guest-client', data),
};
