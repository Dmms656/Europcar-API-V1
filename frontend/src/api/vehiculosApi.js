import api from './axiosClient';

// Internal admin CRUD (rutas con mayúscula: /api/v1/Vehiculos)
export const vehiculosApi = {
  getAll: () => api.get('/Vehiculos'),
  getDisponibles: (params) => api.get('/Vehiculos/disponibles', { params }),
  getById: (id) => api.get(`/Vehiculos/${id}`),
  create: (data) => api.post('/Vehiculos', data),
  update: (id, data) => api.put(`/Vehiculos/${id}`, data),
  delete: (id) => api.delete(`/Vehiculos/${id}`),
};
