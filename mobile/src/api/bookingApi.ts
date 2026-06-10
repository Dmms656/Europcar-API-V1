import api from './axiosClient';

export const bookingApi = {
  buscarVehiculos: (params: Record<string, unknown>) => api.get('/vehiculos', { params }),
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
