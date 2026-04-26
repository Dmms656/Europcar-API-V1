import { create } from 'zustand';

export const useAuthStore = create((set, get) => ({
  token: localStorage.getItem('token') || null,
  user: JSON.parse(localStorage.getItem('user') || 'null'),
  isAuthenticated: !!localStorage.getItem('token'),

  login: (loginResponse) => {
    const userData = {
      username: loginResponse.username,
      correo: loginResponse.correo,
      roles: loginResponse.roles,
      expiration: loginResponse.expiration,
    };
    localStorage.setItem('token', loginResponse.token);
    localStorage.setItem('user', JSON.stringify(userData));
    set({ token: loginResponse.token, user: userData, isAuthenticated: true });
  },

  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    set({ token: null, user: null, isAuthenticated: false });
  },

  hasRole: (role) => {
    const user = get().user;
    return user?.roles?.includes(role) || false;
  },

  hasAnyRole: (...roles) => {
    const user = get().user;
    return roles.some((role) => user?.roles?.includes(role));
  },
}));
