import api from './axiosClient';

export const vehiculosApi = {
  getAll: () => api.get('/admin/Vehiculos'),
  getDisponibles: (params?: Record<string, unknown>) =>
    api.get('/admin/Vehiculos/disponibles', { params }),
  getById: (id: number | string) => api.get(`/admin/Vehiculos/${id}`),
  create: (data: Record<string, unknown>) => api.post('/admin/Vehiculos', data),
  update: (id: number | string, data: Record<string, unknown>) => api.put(`/admin/Vehiculos/${id}`, data),
  cambiarEstadoOperativo: (id: number | string, estadoOperativo: string) =>
    api.put(`/admin/Vehiculos/${id}/estado-operativo`, { estadoOperativo }),
  delete: (id: number | string) => api.delete(`/admin/Vehiculos/${id}`),
};
