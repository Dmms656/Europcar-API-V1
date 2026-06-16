import api from './axiosClient';

export const localizacionesApi = {
  getAll: (soloActivas = false) => api.get('/admin/Localizaciones', { params: { soloActivas } }),
  getById: (id: number | string) => api.get(`/admin/Localizaciones/${id}`),
  getCiudades: () => api.get('/admin/Localizaciones/ciudades'),
  create: (data: Record<string, unknown>) => api.post('/admin/Localizaciones', data),
  update: (id: number | string, data: Record<string, unknown>) => api.put(`/admin/Localizaciones/${id}`, data),
  cambiarEstado: (id: number | string, estado: string, motivo?: string) =>
    api.put(`/admin/Localizaciones/${id}/estado`, { estado, motivo }),
  delete: (id: number | string) => api.delete(`/admin/Localizaciones/${id}`),
};
