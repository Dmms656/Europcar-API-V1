import api from './axiosClient';

export type ContratoItem = {
  idContrato?: number;
  id?: number;
  codigoContrato?: string;
  numero?: string;
  estadoContrato?: string;
  estado?: string;
  placaVehiculo?: string;
  vehiculo?: string;
  fechaHoraSalida?: string;
  fechaSalida?: string;
  fechaHoraDevolucion?: string;
  fechaDevolucion?: string;
  totalContrato?: number;
  total?: number;
  nombreCliente?: string;
  kmSalida?: number;
  kmEntrega?: number;
  depositoGarantia?: number;
  deposito?: number;
};

export const contratosApi = {
  getAll: () => api.get('/Contratos'),
  getMisContratos: () => api.get('/Contratos/mis-contratos'),
  getById: (id: number) => api.get(`/Contratos/${id}`),
  create: (data: Record<string, unknown>) => api.post('/Contratos', data),
  update: (id: number, data: Record<string, unknown>) => api.put(`/Contratos/${id}`, data),
  checkout: (data: Record<string, unknown>) => api.post('/Contratos/checkout', data),
  checkin: (data: Record<string, unknown>) => api.post('/Contratos/checkin', data),
};
