import api from './axiosClient';

export const mantenimientosApi = {
  getAll: () => api.get('/Mantenimientos'),
  getById: (id) => api.get(`/Mantenimientos/${id}`),
  getByVehiculo: (idVehiculo) => api.get(`/Mantenimientos/vehiculo/${idVehiculo}`),
  create: (data) => api.post('/Mantenimientos', data),
  update: (id, data) => api.put(`/Mantenimientos/${id}`, data),
  cerrar: (id, data) => api.put(`/Mantenimientos/${id}/cerrar`, data),
};
