import api from './axiosClient';

// Endpoints públicos de Booking (NO requieren JWT)
// Estos controladores usan rutas en minúscula: /vehiculos, /localizaciones, etc.
export const bookingApi = {
  buscarVehiculos: (params) => api.get('/booking/vehiculos', { params }),
  getVehiculoDetalle: (id) => api.get(`/booking/vehiculos/${id}`),
  checkDisponibilidad: (id, params) => api.get(`/booking/vehiculos/${id}/disponibilidad`, { params }),
  getLocalizaciones: (params) => api.get('/localizaciones', { params }),
  getLocalizacionById: (id) => api.get(`/localizaciones/${id}`),
  getCategorias: () => api.get('/categorias'),
  getExtras: () => api.get('/extras'),
};
