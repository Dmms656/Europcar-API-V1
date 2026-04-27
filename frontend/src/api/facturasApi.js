import api from './axiosClient';

export const facturasApi = {
  getMyFacturas: () => api.get('/Facturas/mis-facturas'),
};
