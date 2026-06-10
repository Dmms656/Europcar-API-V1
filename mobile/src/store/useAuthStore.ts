import * as SecureStore from 'expo-secure-store';
import { create } from 'zustand';
import { authApi } from '@/src/api/authApi';
import { unwrapData } from '@/src/utils/apiResponse';

const TOKEN_KEY = 'rc_access_token';
const USER_KEY = 'rc_user';
const USER_TYPE_KEY = 'rc_user_type';

type UserProfile = {
  username?: string;
  correo?: string;
  roles?: string[];
  nombreCompleto?: string;
  idCliente?: number;
};

type AuthState = {
  user: UserProfile | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  userType: 'admin' | 'cliente' | null;
  sessionChecked: boolean;
  login: (loginResponse: UserProfile & { token?: string }, type?: 'admin' | 'cliente') => Promise<void>;
  logout: () => Promise<void>;
  restoreSession: () => Promise<void>;
  refreshProfile: () => Promise<UserProfile | null>;
  clearAuth: () => Promise<void>;
  isAdmin: () => boolean;
};

async function persistAuth(token: string | null, user: UserProfile | null, userType: string | null) {
  if (token) await SecureStore.setItemAsync(TOKEN_KEY, token);
  else await SecureStore.deleteItemAsync(TOKEN_KEY);
  if (user) await SecureStore.setItemAsync(USER_KEY, JSON.stringify(user));
  else await SecureStore.deleteItemAsync(USER_KEY);
  if (userType) await SecureStore.setItemAsync(USER_TYPE_KEY, userType);
  else await SecureStore.deleteItemAsync(USER_TYPE_KEY);
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  accessToken: null,
  isAuthenticated: false,
  userType: null,
  sessionChecked: false,

  login: async (loginResponse, type = 'cliente') => {
    const token = loginResponse.token ?? null;
    const user: UserProfile = { ...loginResponse };
    await persistAuth(token, user, type);
    set({ user, accessToken: token, isAuthenticated: true, userType: type, sessionChecked: true });
  },

  logout: async () => {
    try {
      await authApi.logout();
    } catch {
      /* ignore */
    }
    await persistAuth(null, null, null);
    set({ user: null, accessToken: null, isAuthenticated: false, userType: null, sessionChecked: true });
  },

  clearAuth: async () => {
    await persistAuth(null, null, null);
    set({ user: null, accessToken: null, isAuthenticated: false, userType: null, sessionChecked: true });
  },

  refreshProfile: async () => {
    try {
      const res = await authApi.me();
      const data = unwrapData<UserProfile>(res);
      if (!data) return get().user;
      const roles = data.roles ?? [];
      const isAdmin = roles.some((r) => ['ADMIN', 'AGENTE', 'AGENTE_POS'].includes(r));
      const resolvedType = isAdmin ? 'admin' : 'cliente';
      const token = get().accessToken;
      await persistAuth(token, data, resolvedType);
      set({ user: data, userType: resolvedType, isAuthenticated: true, sessionChecked: true });
      return data;
    } catch {
      return get().user;
    }
  },

  restoreSession: async () => {
    try {
      const [token, userRaw, userType] = await Promise.all([
        SecureStore.getItemAsync(TOKEN_KEY),
        SecureStore.getItemAsync(USER_KEY),
        SecureStore.getItemAsync(USER_TYPE_KEY),
      ]);

      if (token && userRaw) {
        set({
          accessToken: token,
          user: JSON.parse(userRaw),
          isAuthenticated: true,
          userType: (userType as 'admin' | 'cliente') || 'cliente',
        });
      }

      const res = await authApi.me();
      const data = unwrapData<UserProfile>(res);
      if (data) {
        const roles = data.roles ?? [];
        const isAdmin = roles.some((r) => ['ADMIN', 'AGENTE', 'AGENTE_POS'].includes(r));
        const resolvedType = isAdmin ? 'admin' : 'cliente';
        await persistAuth(token, data, resolvedType);
        set({
          user: data,
          accessToken: token,
          isAuthenticated: true,
          userType: resolvedType,
          sessionChecked: true,
        });
        return;
      }
    } catch {
      await persistAuth(null, null, null);
      set({ user: null, accessToken: null, isAuthenticated: false, userType: null });
    }
    set({ sessionChecked: true });
  },

  isAdmin: () => get().userType === 'admin',
}));
