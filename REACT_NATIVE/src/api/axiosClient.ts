import { Platform } from 'react-native';
import axios from 'axios';
import { resolveApiBaseUrl } from '@/src/config/api';
import { useAuthStore } from '@/src/store/useAuthStore';

const isWeb = Platform.OS === 'web';

const api = axios.create({
  baseURL: resolveApiBaseUrl(),
  headers: { 'Content-Type': 'application/json' },
  timeout: 60000,
  // Cookies HttpOnly del middleware (solo aplica en web)
  withCredentials: isWeb,
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().clearAuth();
    }
    return Promise.reject(error);
  }
);

export default api;
