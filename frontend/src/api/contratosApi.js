import api from './axiosClient';

export const contratosApi = {
  getAll: () => api.get('/Contratos'),
  getMisContratos: () => api.get('/Contratos/mis-contratos'),
  getById: (id) => api.get(`/Contratos/${id}`),
  create: (data) => api.post('/Contratos', data),
  update: (id, data) => api.put(`/Contratos/${id}`, data),
  checkout: (data) => api.post('/Contratos/checkout', data),
  checkin: (data) => api.post('/Contratos/checkin', data),
};
