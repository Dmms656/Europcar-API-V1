import api from './axiosClient';

export const mantenimientosApi = {
  getById: (id) => api.get(`/Mantenimientos/${id}`),
  getByVehiculo: (idVehiculo) => api.get(`/Mantenimientos/vehiculo/${idVehiculo}`),
  create: (data) => api.post('/Mantenimientos', data),
  cerrar: (id, data) => api.put(`/Mantenimientos/${id}/cerrar`, data),
};
