import api from './axiosClient';

export const clientesApi = {
  getAll: (params?: { page?: number; limit?: number }) =>
    api.get('/Clientes', { params: { page: params?.page ?? 1, limit: params?.limit ?? 100 } }),
  getById: (id: number | string) => api.get(`/Clientes/${id}`),
  create: (data: Record<string, unknown>) => api.post('/Clientes', data),
  update: (id: number | string, data: Record<string, unknown>) => api.put(`/Clientes/${id}`, data),
  delete: (id: number | string) => api.delete(`/Clientes/${id}`),
};
