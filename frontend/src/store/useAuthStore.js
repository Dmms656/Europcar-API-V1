import { create } from 'zustand';
import { authApi } from '../api/authApi';

const USER_KEY = 'user';
const USER_TYPE_KEY = 'userType';
/** JWT en sessionStorage (solo esta pestaña; sobrevive F5, no localStorage). */
const TOKEN_KEY = 'rc_access_token';

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

const parseCachedUser = () => {
  const raw = readSession(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
};

/** Estado inicial desde sessionStorage para no perder sesión al recargar. */
const hydrateInitialAuth = () => {
  const token = readSession(TOKEN_KEY);
  const user = parseCachedUser();
  const userType = readSession(USER_TYPE_KEY);
  const hasSession = Boolean(token && user);
  return {
    user: hasSession ? user : null,
    accessToken: token || null,
    isAuthenticated: hasSession,
    userType: hasSession ? userType : null,
    sessionChecked: false,
  };
};

export const useAuthStore = create((set, get) => ({
  ...hydrateInitialAuth(),

  mergeUserProfile: (base, profile = {}) => ({
    username: profile.username ?? base?.username,
    correo: profile.correo ?? base?.correo,
    roles: profile.roles ?? base?.roles,
    expiration: profile.expiration ?? base?.expiration,
    idCliente: profile.idCliente ?? base?.idCliente,
    nombreCompleto: profile.nombreCompleto ?? base?.nombreCompleto,
    numeroIdentificacion: profile.numeroIdentificacion ?? base?.numeroIdentificacion,
    nombres: profile.nombres ?? base?.nombres,
    apellidos: profile.apellidos ?? base?.apellidos,
    telefono: profile.telefono ?? base?.telefono,
  }),

  login: (loginResponse, type = 'admin') => {
    const userData = get().mergeUserProfile(null, loginResponse);
    const token = loginResponse.token || null;
    writeSession(USER_KEY, JSON.stringify(userData));
    writeSession(USER_TYPE_KEY, type);
    if (token) writeSession(TOKEN_KEY, token);
    else writeSession(TOKEN_KEY, null);
    set({
      user: userData,
      accessToken: token,
      isAuthenticated: true,
      userType: type,
      sessionChecked: true,
    });
  },

  logout: async () => {
    try {
      await authApi.logout({ suppressErrorToast: true, suppressAuthRedirect: true });
    } catch {
      /* cookie puede ya estar ausente */
    }
    writeSession(USER_KEY, null);
    writeSession(USER_TYPE_KEY, null);
    writeSession(TOKEN_KEY, null);
    set({
      user: null,
      accessToken: null,
      isAuthenticated: false,
      userType: null,
      sessionChecked: true,
    });
  },

  restoreSession: async () => {
    const cachedToken = readSession(TOKEN_KEY);
    const cachedUser = parseCachedUser();
    const cachedType = readSession(USER_TYPE_KEY);

    if (cachedToken && cachedUser) {
      set({
        user: cachedUser,
        accessToken: cachedToken,
        isAuthenticated: true,
        userType: cachedType,
      });
    }

    try {
      const res = await authApi.me({ suppressErrorToast: true, suppressAuthRedirect: true });
      const data = res.data?.data;
      if (!data) {
        if (cachedToken && cachedUser) {
          set({ sessionChecked: true });
          return;
        }
        set({ user: null, accessToken: null, isAuthenticated: false, userType: null, sessionChecked: true });
        return;
      }
      const isAdmin = data.roles?.some((r) => ['ADMIN', 'AGENTE', 'AGENTE_POS'].includes(r));
      const userType = isAdmin ? 'admin' : 'cliente';
      const userData = get().mergeUserProfile(get().user, data);
      writeSession(USER_KEY, JSON.stringify(userData));
      writeSession(USER_TYPE_KEY, userType);
      set({
        user: userData,
        accessToken: cachedToken || get().accessToken,
        isAuthenticated: true,
        userType,
        sessionChecked: true,
      });
    } catch (err) {
      const status = err?.response?.status;
      if (status === 401) {
        writeSession(USER_KEY, null);
        writeSession(USER_TYPE_KEY, null);
        writeSession(TOKEN_KEY, null);
        set({
          user: null,
          accessToken: null,
          isAuthenticated: false,
          userType: null,
          sessionChecked: true,
        });
        return;
      }
      if (cachedToken && cachedUser) {
        set({ sessionChecked: true });
        return;
      }
      writeSession(USER_KEY, null);
      writeSession(USER_TYPE_KEY, null);
      writeSession(TOKEN_KEY, null);
      set({
        user: null,
        accessToken: null,
        isAuthenticated: false,
        userType: null,
        sessionChecked: true,
      });
    }
  },

  clearAuth: () => {
    writeSession(USER_KEY, null);
    writeSession(USER_TYPE_KEY, null);
    writeSession(TOKEN_KEY, null);
    set({
      user: null,
      accessToken: null,
      isAuthenticated: false,
      userType: null,
      sessionChecked: true,
    });
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
