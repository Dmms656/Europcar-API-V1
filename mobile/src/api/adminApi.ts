import api from './axiosClient';

export const adminVehiculosApi = {
  getAll: () => api.get('/admin/Vehiculos'),
  getById: (id: number | string) => api.get(`/admin/Vehiculos/${id}`),
};

export const adminClientesApi = {
  getAll: (params?: Record<string, unknown>) => api.get('/admin/Clientes', { params }),
  getById: (id: number | string) => api.get(`/admin/Clientes/${id}`),
};

export const adminReservasApi = {
  getAll: (params?: Record<string, unknown>) => api.get('/admin/Reservas', { params }),
  getByCodigo: (codigo: string) => api.get(`/admin/Reservas/${codigo}`),
};
