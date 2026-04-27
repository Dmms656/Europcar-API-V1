import api from './axiosClient';

export const authApi = {
  login: (credentials, config = {}) => api.post('/Auth/login', credentials, config),
  register: (data, config = {}) => api.post('/Auth/register', data, config),
  cedulaExists: (cedula) => api.get('/Auth/cedula-exists', { params: { cedula } }),
};
