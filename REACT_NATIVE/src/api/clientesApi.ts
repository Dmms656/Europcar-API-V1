import api from './axiosClient';

export const clientesApi = {
  getAll: () => api.get('/Clientes'),
  getById: (id: number | string) => api.get(`/Clientes/${id}`),
  create: (data: Record<string, unknown>) => api.post('/Clientes', data),
  update: (id: number | string, data: Record<string, unknown>) => api.put(`/Clientes/${id}`, data),
  delete: (id: number | string) => api.delete(`/Clientes/${id}`),
};
