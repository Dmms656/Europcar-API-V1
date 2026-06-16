import api from './axiosClient';

export const catalogosApi = {
  getPaises: () => api.get('/Catalogos/paises'),
  getPaisById: (id: number | string) => api.get(`/Catalogos/paises/${id}`),
  createPais: (data: Record<string, unknown>) => api.post('/Catalogos/paises', data),
  updatePais: (id: number | string, data: Record<string, unknown>) => api.put(`/Catalogos/paises/${id}`, data),
  cambiarEstadoPais: (id: number | string, estado: string, motivo?: string) =>
    api.put(`/Catalogos/paises/${id}/estado`, { estado, motivo }),
  deletePais: (id: number | string) => api.delete(`/Catalogos/paises/${id}`),
  getCiudades: () => api.get('/Catalogos/ciudades'),
  getCiudadById: (id: number | string) => api.get(`/Catalogos/ciudades/${id}`),
  createCiudad: (data: Record<string, unknown>) => api.post('/Catalogos/ciudades', data),
  updateCiudad: (id: number | string, data: Record<string, unknown>) => api.put(`/Catalogos/ciudades/${id}`, data),
  cambiarEstadoCiudad: (id: number | string, estado: string, motivo?: string) =>
    api.put(`/Catalogos/ciudades/${id}/estado`, { estado, motivo }),
  deleteCiudad: (id: number | string) => api.delete(`/Catalogos/ciudades/${id}`),
  getExtras: () => api.get('/Catalogos/extras'),
  getExtraById: (id: number | string) => api.get(`/Catalogos/extras/${id}`),
  createExtra: (data: Record<string, unknown>) => api.post('/Catalogos/extras', data),
  updateExtra: (id: number | string, data: Record<string, unknown>) => api.put(`/Catalogos/extras/${id}`, data),
  cambiarEstadoExtra: (id: number | string, estado: string, motivo?: string) =>
    api.put(`/Catalogos/extras/${id}/estado`, { estado, motivo }),
  deleteExtra: (id: number | string) => api.delete(`/Catalogos/extras/${id}`),
};
