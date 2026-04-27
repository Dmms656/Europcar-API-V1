import api from './axiosClient';

export const usuariosApi = {
  getAll: () => api.get('/Usuarios'),
  getById: (id) => api.get(`/Usuarios/${id}`),
  create: (data) => api.post('/Usuarios', data),
  updateEstado: (id, estado) => api.put(`/Usuarios/${id}/estado`, { estado }),
  updateRoles: (id, roles) => api.put(`/Usuarios/${id}/roles`, { roles }),
  delete: (id) => api.delete(`/Usuarios/${id}`),
};
