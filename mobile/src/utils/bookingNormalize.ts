/** Normalización alineada con frontend/src/utils/bookingNormalize.js */

function refNombre(ref: unknown): string {
  if (ref == null) return '';
  if (typeof ref === 'string') return ref;
  if (typeof ref === 'object') {
    const o = ref as Record<string, unknown>;
    return String(
      o.nombre ?? o.Nombre ?? o.nombreMarca ?? o.NombreMarca ?? o.nombreCategoria ?? o.NombreCategoria ?? '',
    );
  }
  return String(ref);
}

function refId(ref: Record<string, unknown>, ...keys: string[]) {
  for (const k of keys) {
    if (ref[k] != null) return ref[k];
  }
  return null;
}

function sucursalNombrePlano(localizacion: unknown): string {
  if (localizacion == null) return '';
  if (typeof localizacion === 'string') return localizacion;
  const loc = localizacion as Record<string, unknown>;
  return String(
    loc.nombre ?? loc.Nombre ?? loc.nombreLocalizacion ?? loc.NombreLocalizacion ?? '',
  );
}

export function toDateTimeLocalValue(d: Date) {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

export function defaultRentalDateTimeLocalRange() {
  const start = new Date();
  start.setDate(start.getDate() + 1);
  start.setHours(10, 0, 0, 0);
  const end = new Date(start);
  end.setDate(end.getDate() + 2);
  end.setHours(10, 0, 0, 0);
  return {
    fechaRecogida: toDateTimeLocalValue(start),
    fechaDevolucion: toDateTimeLocalValue(end),
  };
}

export type VehiculoBooking = {
  idVehiculo?: number;
  id?: number;
  marca?: string;
  modelo?: string;
  modeloVehiculo?: string;
  categoria?: string;
  nombreCategoria?: string;
  tipoCombustible?: string;
  combustible?: string;
  tipoTransmision?: string;
  transmision?: string;
  precioBaseDia?: number;
  precioDia?: number;
  imagenUrl?: string;
  nombreSucursal?: string;
  localizacion?: string | Record<string, unknown>;
  idLocalizacion?: number;
  disponible?: boolean;
  codigoInterno?: string;
};

/** Rellena datos del cliente desde perfil de sesión. */
export function guestFormFromUserProfile(user: {
  nombres?: string;
  apellidos?: string;
  nombreCompleto?: string;
  username?: string;
  numeroIdentificacion?: string;
  correo?: string;
  telefono?: string;
} | null) {
  if (!user) return null;
  const nombres = (user.nombres || '').trim();
  const apellidos = (user.apellidos || '').trim();
  let nombre = nombres;
  let apellido = apellidos;
  if (!nombre && user.nombreCompleto) {
    const parts = user.nombreCompleto.trim().split(/\s+/).filter(Boolean);
    nombre = parts[0] || '';
    apellido = apellido || (parts.length > 1 ? parts.slice(1).join(' ') : '');
  }
  return {
    nombre,
    apellido,
    cedula: (user.numeroIdentificacion || '').trim(),
    correo: (user.correo || '').trim(),
    telefono: (user.telefono || '').trim(),
    direccion: '',
  };
}

export function normalizeContactoReserva(
  user: { correo?: string; username?: string; numeroIdentificacion?: string } | null,
  guestForm: { correo?: string; telefono?: string; cedula?: string },
) {
  const correoRaw = (guestForm.correo || user?.correo || '').trim();
  const telefonoRaw = (guestForm.telefono || '').trim();
  const slug =
    String(user?.username || guestForm.cedula || 'cliente')
      .replace(/\W/g, '')
      .slice(0, 24) || 'cliente';
  const correo = correoRaw && correoRaw.includes('@') ? correoRaw : `${slug}@reserva.europcar.ec`;
  const telefono = telefonoRaw.length >= 7 ? telefonoRaw : '0999999999';
  return { correo, telefono };
}

export function normalizeVehiculoDetalle(v: Record<string, unknown> | null) {
  if (!v) return null;
  const loc = v.localizacion;
  const precioObj = v.precio as Record<string, unknown> | undefined;
  const precioBase =
    Number(v.precioBaseDia ?? v.precioDia ?? precioObj?.precioBaseDia ?? precioObj?.PrecioBaseDia ?? 0) || 0;
  const norm = normalizeVehiculoFromBookingList(v);
  return {
    ...norm,
    precioBaseDia: precioBase,
    precioDia: Number(v.precioDia ?? precioBase) || precioBase,
    imagenUrl: String(v.imagenUrl ?? v.ImagenUrl ?? norm.imagenUrl ?? '') || undefined,
  };
}

export function normalizeVehiculoFromBookingList(v: Record<string, unknown>): VehiculoBooking {
  const loc = v.localizacion;
  const idLoc =
    v.idLocalizacion ??
    (typeof loc === 'object' && loc
      ? refId(loc as Record<string, unknown>, 'idLocalizacion', 'IdLocalizacion', 'id', 'Id')
      : null);

  const marcaStr = refNombre(v.marca);
  const catStr = refNombre(v.categoria) || String(v.nombreCategoria ?? v.NombreCategoria ?? '');

  return {
    ...(v as VehiculoBooking),
    idVehiculo: Number(v.idVehiculo ?? v.id ?? 0) || undefined,
    marca: marcaStr,
    categoria: catStr,
    modelo: String(v.modelo ?? v.modeloVehiculo ?? ''),
    tipoCombustible: String(v.tipoCombustible ?? v.combustible ?? ''),
    tipoTransmision: String(v.tipoTransmision ?? v.transmision ?? ''),
    nombreSucursal: sucursalNombrePlano(loc),
    localizacion: sucursalNombrePlano(loc) || (typeof loc === 'string' ? loc : ''),
    idLocalizacion: idLoc != null ? Number(idLoc) : undefined,
    precioDia: Number(v.precioBaseDia ?? v.precioDia ?? 0) || undefined,
    imagenUrl: String(v.imagenUrl ?? v.ImagenUrl ?? '') || undefined,
  };
}

export function getPayload<T extends Record<string, unknown>>(response: { data?: unknown }): T {
  const body = response.data as { data?: T; Data?: T } | undefined;
  return (body?.data ?? body?.Data ?? {}) as T;
}

const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

/** Agrega flota desde booking API sin saturar el catálogo (429). */
export async function loadCatalogFromBooking(
  localizaciones: { idLocalizacion: number }[],
  buscar: (params: Record<string, unknown>) => Promise<{ data?: unknown }>,
  options?: { maxLocations?: number; delayMs?: number },
) {
  const { fechaRecogida, fechaDevolucion } = defaultRentalDateTimeLocalRange();
  const maxLocations = options?.maxLocations ?? 8;
  const delayMs = options?.delayMs ?? 350;
  const byId = new Map<number, VehiculoBooking>();

  for (const loc of localizaciones.slice(0, maxLocations)) {
    try {
      const res = await buscar({
        idLocalizacion: loc.idLocalizacion,
        fechaRecogida,
        fechaDevolucion,
        page: 1,
        limit: 50,
      });
      const payload = getPayload<{ vehiculos?: Record<string, unknown>[] }>(res);
      const raw = payload.vehiculos ?? [];
      raw.forEach((v) => {
        const norm = normalizeVehiculoFromBookingList(v);
        const key = norm.idVehiculo ?? norm.id;
        if (key != null && !byId.has(key)) byId.set(key, norm);
      });
    } catch {
      /* continuar con otras sucursales */
    }
    await sleep(delayMs);
  }

  return Array.from(byId.values());
}
