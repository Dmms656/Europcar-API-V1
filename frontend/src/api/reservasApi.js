import api from './axiosClient';

// API interna de reservas (back-office) montada en /api/v1/admin/Reservas.
// La ruta pública /api/v1/reservas queda reservada para el contrato Booking.
export const reservasApi = {
  create: (data) => api.post('/admin/Reservas', data),
  update: (id, data) => api.put(`/admin/Reservas/${id}`, data),
  getByCodigo: (codigo) => api.get(`/admin/Reservas/${codigo}`),
  getByCliente: (idCliente) => api.get(`/admin/Reservas/cliente/${idCliente}`),
  confirmar: (id, data) => api.put(`/admin/Reservas/${id}/confirmar`, data || {}),
  cancelar: (id, motivo) => api.put(`/admin/Reservas/${id}/cancelar`, { motivo }),
  guestClient: (data) => api.post('/admin/Reservas/guest-client', data),
};
