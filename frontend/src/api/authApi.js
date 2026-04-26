import api from './axiosClient';

export const authApi = {
  login: (credentials) => api.post('/Auth/login', credentials),
  register: (data) => api.post('/Auth/register', data),
};
