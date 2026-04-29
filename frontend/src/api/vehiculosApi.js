import api from './axiosClient';

// Internal admin CRUD montado en /api/v1/admin/Vehiculos.
// La ruta pública /api/v1/Vehiculos quedó reservada para el contrato Booking.
export const vehiculosApi = {
  getAll: () => api.get('/admin/Vehiculos'),
  getDisponibles: (params) => api.get('/admin/Vehiculos/disponibles', { params }),
  getById: (id) => api.get(`/admin/Vehiculos/${id}`),
  create: (data) => api.post('/admin/Vehiculos', data),
  update: (id, data) => api.put(`/admin/Vehiculos/${id}`, data),
  cambiarEstadoOperativo: (id, estadoOperativo) => api.put(`/admin/Vehiculos/${id}/estado-operativo`, { estadoOperativo }),
  delete: (id) => api.delete(`/admin/Vehiculos/${id}`),
};
