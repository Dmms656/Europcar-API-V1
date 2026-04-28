import api from './axiosClient';

export const catalogosApi = {
  getLocalizaciones: () => api.get('/Catalogos/localizaciones'),
  getLocalizacionById: (id) => api.get(`/Catalogos/localizaciones/${id}`),
  getCategorias: () => api.get('/Catalogos/categorias'),
  getMarcas: () => api.get('/Catalogos/marcas'),
  getExtras: () => api.get('/Catalogos/extras'),
  getExtraById: (id) => api.get(`/Catalogos/extras/${id}`),
  createExtra: (data) => api.post('/Catalogos/extras', data),
  updateExtra: (id, data) => api.put(`/Catalogos/extras/${id}`, data),
  cambiarEstadoExtra: (id, estado, motivo) => api.put(`/Catalogos/extras/${id}/estado`, { estado, motivo }),
  deleteExtra: (id) => api.delete(`/Catalogos/extras/${id}`),
};
