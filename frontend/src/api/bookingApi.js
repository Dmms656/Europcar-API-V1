import api from './axiosClient';

// Endpoints públicos del contrato Booking / RedCar (no requieren JWT).
// Rutas literales del contrato en minúscula bajo /api/v1/...
export const bookingApi = {
  // Vehículos (endpoints 1, 2, 3)
  buscarVehiculos: (params) => api.get('/vehiculos', { params }),
  getVehiculoDetalle: (id) => api.get(`/vehiculos/${id}`),
  checkDisponibilidad: (id, params) => api.get(`/vehiculos/${id}/disponibilidad`, { params }),

  // Catálogos (endpoints 4, 5, 6, 7)
  getLocalizaciones: (params) => api.get('/localizaciones', { params }),
  getLocalizacionById: (id) => api.get(`/localizaciones/${id}`),
  getCiudades: () => api.get('/ciudades'),
  getCategorias: () => api.get('/categorias'),
  getExtras: () => api.get('/extras'),

  // Reservas (endpoints 8, 9, 10, 11)
  crearReserva: (data) => api.post('/reservas', data),
  getReservaByCodigo: (codigoReserva) => api.get(`/reservas/${codigoReserva}`),
  cancelarReserva: (codigoReserva, data) => api.patch(`/reservas/${codigoReserva}/cancelar`, data),
  getFacturaPorReserva: (codigoReserva) => api.get(`/reservas/${codigoReserva}/factura`),
};
