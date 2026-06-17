import { create } from 'zustand';
import { authApi } from '@/src/api/authApi';
import { unwrapData } from '@/src/utils/apiResponse';
import { multiGet, setItem, removeItem } from '@/src/utils/storage';

const TOKEN_KEY = 'rc_access_token';
const USER_KEY = 'rc_user';
const USER_TYPE_KEY = 'rc_user_type';

export type UserProfile = {
  username?: string;
  correo?: string;
  roles?: string[];
  nombreCompleto?: string;
  idCliente?: number;
  telefono?: string;
  direccion?: string;
  numeroIdentificacion?: string;
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
  hasAdminRole: () => boolean;
  hasRole: (role: string) => boolean;
  hasAnyRole: (...roles: string[]) => boolean;
};

function hasAdminRoleInRoles(roles?: string[]) {
  return roles?.some((r) => ['ADMIN', 'AGENTE', 'AGENTE_POS'].includes(r)) ?? false;
}

function resolveUserType(stored: 'admin' | 'cliente' | null, roles?: string[]): 'admin' | 'cliente' {
  if (stored === 'cliente') return 'cliente';
  const canAdmin = hasAdminRoleInRoles(roles);
  if (stored === 'admin' && canAdmin) return 'admin';
  return canAdmin ? 'admin' : 'cliente';
}

async function persistAuth(token: string | null, user: UserProfile | null, userType: string | null) {
  if (token) await setItem(TOKEN_KEY, token);
  else await removeItem(TOKEN_KEY);
  if (user) await setItem(USER_KEY, JSON.stringify(user));
  else await removeItem(USER_KEY);
  if (userType) await setItem(USER_TYPE_KEY, userType);
  else await removeItem(USER_TYPE_KEY);
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
      const stored = get().userType;
      const resolvedType = resolveUserType(stored, data.roles);
      const token = get().accessToken;
      await persistAuth(token, data, resolvedType);
      set({ user: data, userType: resolvedType, isAuthenticated: true, sessionChecked: true });
      return data;
    } catch {
      return get().user;
    }
  },

  restoreSession: async () => {
    let token: string | null = null;
    let storedType: 'admin' | 'cliente' | null = null;

    try {
      const [t, userRaw, userTypeRaw] = await multiGet([TOKEN_KEY, USER_KEY, USER_TYPE_KEY]);
      token = t;
      storedType = (userTypeRaw as 'admin' | 'cliente') || null;

      if (token && userRaw) {
        set({
          accessToken: token,
          user: JSON.parse(userRaw),
          isAuthenticated: true,
          userType: storedType || 'cliente',
        });
      }

      if (!token) {
        set({ sessionChecked: true });
        return;
      }

      const res = await authApi.me();
      const data = unwrapData<UserProfile>(res);
      if (data) {
        const resolvedType = resolveUserType(storedType, data.roles);
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
      if (token) {
        set({ sessionChecked: true });
        return;
      }
      await persistAuth(null, null, null);
      set({ user: null, accessToken: null, isAuthenticated: false, userType: null });
    }
    set({ sessionChecked: true });
  },

  isAdmin: () => get().userType === 'admin',
  hasAdminRole: () => hasAdminRoleInRoles(get().user?.roles),
  hasRole: (role: string) => get().user?.roles?.includes(role) ?? false,
  hasAnyRole: (...roles: string[]) => roles.some((r) => get().user?.roles?.includes(r)),
}));
