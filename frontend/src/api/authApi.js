import api from './axiosClient';

export const authApi = {
  login: (credentials, config = {}) => api.post('/Auth/login', credentials, config),
  register: (data, config = {}) => api.post('/Auth/register', data, config),
  logout: (config = {}) => api.post('/Auth/logout', {}, config),
  me: (config = {}) => api.get('/Auth/me', config),
  cedulaExists: (cedula) => api.get('/Auth/cedula-exists', { params: { cedula } }),
};
