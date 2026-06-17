import api from './axiosClient';

export const conductoresApi = {
  getByCliente: (idCliente: number | string) => api.get(`/Conductores/cliente/${idCliente}`),
  getById: (id: number | string) => api.get(`/Conductores/${id}`),
  create: (data: Record<string, unknown>) => api.post('/Conductores', data),
  update: (id: number | string, data: Record<string, unknown>) => api.put(`/Conductores/${id}`, data),
  delete: (id: number | string) => api.delete(`/Conductores/${id}`),
};
