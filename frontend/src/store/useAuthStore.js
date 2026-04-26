import { create } from 'zustand';

const deriveUserType = () => {
  const stored = localStorage.getItem('userType');
  if (stored) return stored;
  // Fallback: derive from roles for old sessions
  try {
    const user = JSON.parse(localStorage.getItem('user') || 'null');
    if (user?.roles?.some(r => ['ADMIN', 'AGENTE_POS'].includes(r))) return 'admin';
    if (user) return 'cliente';
  } catch { /* ignore */ }
  return null;
};

export const useAuthStore = create((set, get) => ({
  token: localStorage.getItem('token') || null,
  user: JSON.parse(localStorage.getItem('user') || 'null'),
  isAuthenticated: !!localStorage.getItem('token'),
  userType: deriveUserType(), // 'admin' | 'cliente'

  login: (loginResponse, type = 'admin') => {
    const userData = {
      username: loginResponse.username,
      correo: loginResponse.correo,
      roles: loginResponse.roles,
      expiration: loginResponse.expiration,
      // Client-specific fields
      idCliente: loginResponse.idCliente,
      nombreCompleto: loginResponse.nombreCompleto,
    };
    localStorage.setItem('token', loginResponse.token);
    localStorage.setItem('user', JSON.stringify(userData));
    localStorage.setItem('userType', type);
    set({ token: loginResponse.token, user: userData, isAuthenticated: true, userType: type });
  },

  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('userType');
    set({ token: null, user: null, isAuthenticated: false, userType: null });
  },

  isAdmin: () => {
    const { userType } = get();
    return userType === 'admin';
  },

  isCliente: () => {
    const { userType } = get();
    return userType === 'cliente';
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
