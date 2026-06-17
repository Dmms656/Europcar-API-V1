import { create } from 'zustand';
import { authApi } from '@/src/api/authApi';
import { unwrapData } from '@/src/utils/apiResponse';
import { mergeUserProfile, normalizeAuthProfile } from '@/src/utils/authProfile';
import { clienteDtoToGuestForm } from '@/src/utils/bookingNormalize';
import { multiGet, setItem, removeItem } from '@/src/utils/storage';

const TOKEN_KEY = 'rc_access_token';
const USER_KEY = 'rc_user';
const USER_TYPE_KEY = 'rc_user_type';

export type UserProfile = {
  username?: string;
  correo?: string;
  roles?: string[];
  nombreCompleto?: string;
  nombres?: string;
  apellidos?: string;
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
  patchUser: (partial: UserProfile) => Promise<void>;
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

async function enrichUserFromClienteApi(user: UserProfile | null): Promise<UserProfile | null> {
  if (!user?.idCliente) return user;
  if (user.numeroIdentificacion?.trim()) return user;
  try {
    const { clientesApi } = await import('@/src/api/clientesApi');
    const res = await clientesApi.getById(user.idCliente);
    const dto = unwrapData<Record<string, unknown>>(res);
    const fromCliente = clienteDtoToGuestForm(dto);
    if (!fromCliente) return user;
    return (
      mergeUserProfile(user, {
        nombres: fromCliente.nombre,
        apellidos: fromCliente.apellido,
        numeroIdentificacion: fromCliente.cedula,
        telefono: fromCliente.telefono,
        correo: fromCliente.correo,
        nombreCompleto:
          [fromCliente.nombre, fromCliente.apellido].filter(Boolean).join(' ').trim() || user.nombreCompleto,
      }) ?? user
    );
  } catch {
    return user;
  }
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
    const incoming = normalizeAuthProfile(loginResponse as UserProfile & Record<string, unknown>);
    const user: UserProfile = mergeUserProfile(null, incoming) ?? { ...loginResponse };
    await persistAuth(token, user, type);
    set({ user, accessToken: token, isAuthenticated: true, userType: type, sessionChecked: true });
    try {
      await get().refreshProfile();
    } catch {
      /* login ya dejó sesión usable */
    }
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

  patchUser: async (partial) => {
    const merged = mergeUserProfile(get().user, partial);
    if (!merged) return;
    await persistAuth(get().accessToken, merged, get().userType);
    set({ user: merged });
  },

  clearAuth: async () => {
    await persistAuth(null, null, null);
    set({ user: null, accessToken: null, isAuthenticated: false, userType: null, sessionChecked: true });
  },

  refreshProfile: async () => {
    try {
      const res = await authApi.me();
      const incoming = normalizeAuthProfile(unwrapData<Record<string, unknown>>(res));
      if (!incoming) return get().user;
      let merged = mergeUserProfile(get().user, incoming);
      merged = (await enrichUserFromClienteApi(merged)) ?? merged;
      const stored = get().userType;
      const resolvedType = resolveUserType(stored, merged?.roles);
      const token = get().accessToken;
      await persistAuth(token, merged, resolvedType);
      set({ user: merged, userType: resolvedType, isAuthenticated: true, sessionChecked: true });
      return merged;
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
      const incoming = normalizeAuthProfile(unwrapData<Record<string, unknown>>(res));
      if (incoming) {
        let merged = mergeUserProfile(get().user, incoming);
        merged = (await enrichUserFromClienteApi(merged)) ?? merged;
        const resolvedType = resolveUserType(storedType, merged?.roles);
        await persistAuth(token, merged, resolvedType);
        set({
          user: merged,
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
