import type { UserProfile } from '@/src/store/useAuthStore';

function str(v: unknown): string {
  return v == null ? '' : String(v).trim();
}

/** Normaliza respuestas de login / Auth/me (camelCase o PascalCase). */
export function normalizeAuthProfile(raw: Record<string, unknown> | null | undefined): UserProfile | null {
  if (!raw) return null;
  const idRaw = raw.idCliente ?? raw.IdCliente;
  const rolesRaw = raw.roles ?? raw.Roles;
  const roles = Array.isArray(rolesRaw)
    ? rolesRaw.map(String)
    : rolesRaw
      ? [String(rolesRaw)]
      : undefined;

  return {
    username: str(raw.username ?? raw.Username) || undefined,
    correo: str(raw.correo ?? raw.Correo) || undefined,
    roles,
    nombreCompleto: str(raw.nombreCompleto ?? raw.NombreCompleto) || undefined,
    nombres: str(raw.nombres ?? raw.Nombres ?? raw.nombre1 ?? raw.Nombre1) || undefined,
    apellidos: str(raw.apellidos ?? raw.Apellidos ?? raw.apellido1 ?? raw.Apellido1) || undefined,
    idCliente: idRaw != null && idRaw !== '' ? Number(idRaw) : undefined,
    telefono: str(raw.telefono ?? raw.Telefono) || undefined,
    direccion: str(raw.direccion ?? raw.Direccion ?? raw.direccionPrincipal ?? raw.DireccionPrincipal) || undefined,
    numeroIdentificacion: str(raw.numeroIdentificacion ?? raw.NumeroIdentificacion) || undefined,
  };
}

/** Conserva datos previos cuando /me no devuelve todos los campos. */
export function mergeUserProfile(base: UserProfile | null, incoming: UserProfile | null): UserProfile | null {
  if (!incoming && !base) return null;
  if (!base) return incoming;
  if (!incoming) return base;
  return {
    username: incoming.username || base.username,
    correo: incoming.correo || base.correo,
    roles: incoming.roles?.length ? incoming.roles : base.roles,
    nombreCompleto: incoming.nombreCompleto || base.nombreCompleto,
    nombres: incoming.nombres || base.nombres,
    apellidos: incoming.apellidos || base.apellidos,
    idCliente: incoming.idCliente ?? base.idCliente,
    telefono: incoming.telefono || base.telefono,
    direccion: incoming.direccion || base.direccion,
    numeroIdentificacion: incoming.numeroIdentificacion || base.numeroIdentificacion,
  };
}
