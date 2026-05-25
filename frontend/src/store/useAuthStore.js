import { create } from 'zustand';
import { authApi } from '../api/authApi';

const USER_KEY = 'user';
const USER_TYPE_KEY = 'userType';

const readSession = (key) => {
  try {
    return sessionStorage.getItem(key);
  } catch {
    return null;
  }
};

const writeSession = (key, value) => {
  try {
    if (value == null) sessionStorage.removeItem(key);
    else sessionStorage.setItem(key, value);
  } catch {
    /* ignore quota / private mode */
  }
};

const deriveUserType = () => {
  const stored = readSession(USER_TYPE_KEY);
  if (stored) return stored;
  try {
    const user = JSON.parse(readSession(USER_KEY) || 'null');
    if (user?.roles?.some((r) => ['ADMIN', 'AGENTE_POS'].includes(r))) return 'admin';
    if (user) return 'cliente';
  } catch {
    /* ignore */
  }
  return null;
};

const initialUser = (() => {
  try {
    return JSON.parse(readSession(USER_KEY) || 'null');
  } catch {
    return null;
  }
})();

export const useAuthStore = create((set, get) => ({
  user: initialUser,
  isAuthenticated: !!initialUser,
  userType: deriveUserType(),
  sessionChecked: false,

  login: (loginResponse, type = 'admin') => {
    const userData = {
      username: loginResponse.username,
      correo: loginResponse.correo,
      roles: loginResponse.roles,
      expiration: loginResponse.expiration,
      idCliente: loginResponse.idCliente,
      nombreCompleto: loginResponse.nombreCompleto,
    };
    writeSession(USER_KEY, JSON.stringify(userData));
    writeSession(USER_TYPE_KEY, type);
    set({ user: userData, isAuthenticated: true, userType: type, sessionChecked: true });
  },

  logout: async () => {
    try {
      await authApi.logout({ suppressErrorToast: true });
    } catch {
      /* cookie puede ya estar ausente */
    }
    writeSession(USER_KEY, null);
    writeSession(USER_TYPE_KEY, null);
    set({ user: null, isAuthenticated: false, userType: null, sessionChecked: true });
  },

  restoreSession: async () => {
    try {
      const res = await authApi.me({ suppressErrorToast: true });
      const data = res.data?.data;
      if (!data) {
        set({ user: null, isAuthenticated: false, userType: null, sessionChecked: true });
        return;
      }
      const isAdmin = data.roles?.some((r) => ['ADMIN', 'AGENTE', 'AGENTE_POS'].includes(r));
      const userType = isAdmin ? 'admin' : 'cliente';
      const userData = {
        username: data.username,
        correo: data.correo,
        roles: data.roles,
        idCliente: data.idCliente,
        nombreCompleto: data.nombreCompleto,
      };
      writeSession(USER_KEY, JSON.stringify(userData));
      writeSession(USER_TYPE_KEY, userType);
      set({ user: userData, isAuthenticated: true, userType, sessionChecked: true });
    } catch {
      writeSession(USER_KEY, null);
      writeSession(USER_TYPE_KEY, null);
      set({ user: null, isAuthenticated: false, userType: null, sessionChecked: true });
    }
  },

  isAdmin: () => get().userType === 'admin',

  isCliente: () => get().userType === 'cliente',

  hasRole: (role) => {
    const user = get().user;
    return user?.roles?.includes(role) || false;
  },

  hasAnyRole: (...roles) => {
    const user = get().user;
    return roles.some((role) => user?.roles?.includes(role));
  },
}));
