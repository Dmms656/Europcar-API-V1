import api from './axiosClient';

export const usuariosApi = {
  getAll: () => api.get('/Usuarios'),
  getById: (id: number | string) => api.get(`/Usuarios/${id}`),
  create: (data: Record<string, unknown>) => api.post('/Usuarios', data),
  updateEstado: (id: number | string, estado: string) => api.put(`/Usuarios/${id}/estado`, { estado }),
  updateRoles: (id: number | string, roles: string[]) => api.put(`/Usuarios/${id}/roles`, { roles }),
  delete: (id: number | string) => api.delete(`/Usuarios/${id}`),
};
