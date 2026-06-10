import api from './axiosClient';

export const authApi = {
  login: (credentials: { username: string; password: string }) =>
    api.post('/Auth/login', credentials),
  register: (data: Record<string, unknown>) => api.post('/Auth/register', data),
  logout: () => api.post('/Auth/logout', {}),
  me: () => api.get('/Auth/me'),
  cedulaExists: (cedula: string) => api.get('/Auth/cedula-exists', { params: { cedula } }),
  updateProfile: (data: { correo: string; telefono?: string; direccion?: string }) =>
    api.put('/Auth/profile', data),
  changePassword: (data: { currentPassword: string; newPassword: string }) =>
    api.put('/Auth/change-password', data),
};
