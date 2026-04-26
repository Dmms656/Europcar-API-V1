import api from './axiosClient';

export const contratosApi = {
  getAll: () => api.get('/Contratos'),
  getById: (id) => api.get(`/Contratos/${id}`),
  create: (data) => api.post('/Contratos', data),
  checkout: (data) => api.post('/Contratos/checkout', data),
  checkin: (data) => api.post('/Contratos/checkin', data),
};
