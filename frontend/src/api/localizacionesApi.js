import api from './axiosClient';

// Gestión administrativa de localizaciones montada en /api/v1/admin/Localizaciones.
// La ruta pública /api/v1/localizaciones queda reservada para el contrato Booking.
export const localizacionesApi = {
  getAll: (soloActivas = false) => api.get('/admin/Localizaciones', { params: { soloActivas } }),
  getById: (id) => api.get(`/admin/Localizaciones/${id}`),
  getCiudades: () => api.get('/admin/Localizaciones/ciudades'),
  create: (data) => api.post('/admin/Localizaciones', data),
  update: (id, data) => api.put(`/admin/Localizaciones/${id}`, data),
  cambiarEstado: (id, estado, motivo) => api.put(`/admin/Localizaciones/${id}/estado`, { estado, motivo }),
  delete: (id) => api.delete(`/admin/Localizaciones/${id}`),
};
