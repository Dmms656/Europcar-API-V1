import api from './axiosClient';

export const mantenimientosApi = {
  getAll: () => api.get('/Mantenimientos'),
  getById: (id: number | string) => api.get(`/Mantenimientos/${id}`),
  getByVehiculo: (idVehiculo: number | string) => api.get(`/Mantenimientos/vehiculo/${idVehiculo}`),
  create: (data: Record<string, unknown>) => api.post('/Mantenimientos', data),
  update: (id: number | string, data: Record<string, unknown>) => api.put(`/Mantenimientos/${id}`, data),
  cerrar: (id: number | string, data: Record<string, unknown> = {}) =>
    api.put(`/Mantenimientos/${id}/cerrar`, data),
};
