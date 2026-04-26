import api from './axiosClient';

export const clientesApi = {
  getAll: () => api.get('/Clientes'),
  getById: (id) => api.get(`/Clientes/${id}`),
  create: (data) => api.post('/Clientes', data),
  update: (id, data) => api.put(`/Clientes/${id}`, data),
  delete: (id) => api.delete(`/Clientes/${id}`),
};
