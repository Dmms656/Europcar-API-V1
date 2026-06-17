import api from './axiosClient';
import { buildVehiculosSearchParams } from '@/src/utils/bookingNormalize';

export const bookingApi = {
  buscarVehiculos: (params: Record<string, unknown>) => {
    const safe = buildVehiculosSearchParams({
      idLocalizacion: Number(params.idLocalizacion) || 1,
      fechaRecogida: params.fechaRecogida as string | Date | undefined,
      fechaDevolucion: params.fechaDevolucion as string | Date | undefined,
      page: params.page != null ? Number(params.page) : undefined,
      limit: params.limit != null ? Number(params.limit) : undefined,
    });
    return api.get('/vehiculos', { params: safe });
  },
  getVehiculoDetalle: (id: number | string) => api.get(`/vehiculos/${id}`),
  checkDisponibilidad: (id: number | string, params: Record<string, unknown>) =>
    api.get(`/vehiculos/${id}/disponibilidad`, { params }),
  getLocalizaciones: (params?: Record<string, unknown>) => api.get('/localizaciones', { params }),
  getCiudades: () => api.get('/ciudades'),
  getCategorias: () => api.get('/categorias'),
  getExtras: () => api.get('/extras'),
  crearReserva: (data: Record<string, unknown>) => api.post('/reservas', data),
  getReservaByCodigo: (codigo: string) => api.get(`/reservas/${codigo}`),
  cancelarReserva: (codigo: string, data: Record<string, unknown>) =>
    api.patch(`/reservas/${codigo}/cancelar`, data),
};
