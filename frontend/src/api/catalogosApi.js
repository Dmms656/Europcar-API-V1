import api from './axiosClient';

export const catalogosApi = {
  getLocalizaciones: () => api.get('/Catalogos/localizaciones'),
  getLocalizacionById: (id) => api.get(`/Catalogos/localizaciones/${id}`),
  getCategorias: () => api.get('/Catalogos/categorias'),
  getMarcas: () => api.get('/Catalogos/marcas'),
  getExtras: () => api.get('/Catalogos/extras'),
};
