import api from './axiosClient';

export const conductoresApi = {
  getByCliente: (idCliente) => api.get(`/Conductores/cliente/${idCliente}`),
  getById: (id) => api.get(`/Conductores/${id}`),
  create: (data) => api.post('/Conductores', data),
  update: (id, data) => api.put(`/Conductores/${id}`, data),
  delete: (id) => api.delete(`/Conductores/${id}`),
};
