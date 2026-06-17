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

/** ISO 8601 UTC — formato que espera GET /vehiculos (evita 422 por fechas inválidas). */
export function formatBookingApiDate(date: Date): string {
  return date.toISOString();
}

/** Rango interno al listar catálogo (el usuario elige fechas al reservar). */
export function defaultCatalogApiDateRange() {
  const start = new Date();
  start.setDate(start.getDate() + 1);
  start.setHours(10, 0, 0, 0);
  const end = new Date(start);
  end.setDate(end.getDate() + 7);
  end.setHours(10, 0, 0, 0);
  return {
    fechaRecogida: formatBookingApiDate(start),
    fechaDevolucion: formatBookingApiDate(end),
  };
}

function parseBookingDateInput(value?: string | Date | null): Date | null {
  if (value == null) return null;
  if (value instanceof Date) {
    return Number.isNaN(value.getTime()) ? null : value;
  }
  const s = String(value).trim();
  if (!s) return null;
  if (/^\d{4}-\d{2}-\d{2}$/.test(s)) {
    const d = new Date(`${s}T10:00:00`);
    return Number.isNaN(d.getTime()) ? null : d;
  }
  const d = new Date(s);
  return Number.isNaN(d.getTime()) ? null : d;
}

/** Parámetros seguros para GET /vehiculos — siempre fechaDevolucion > fechaRecogida. */
export function buildVehiculosSearchParams(input: {
  idLocalizacion: number;
  fechaRecogida?: string | Date;
  fechaDevolucion?: string | Date;
  page?: number;
  limit?: number;
}) {
  const defaults = defaultCatalogApiDateRange();
  let pickup = parseBookingDateInput(input.fechaRecogida);
  let dropoff = parseBookingDateInput(input.fechaDevolucion);

  if (!pickup) pickup = new Date(defaults.fechaRecogida);
  if (!dropoff) {
    dropoff = new Date(pickup);
    dropoff.setDate(dropoff.getDate() + 3);
    dropoff.setHours(10, 0, 0, 0);
  }
  if (dropoff.getTime() <= pickup.getTime()) {
    dropoff = new Date(pickup);
    dropoff.setDate(dropoff.getDate() + 3);
    dropoff.setHours(10, 0, 0, 0);
  }

  return {
    idLocalizacion: input.idLocalizacion,
    fechaRecogida: formatBookingApiDate(pickup),
    fechaDevolucion: formatBookingApiDate(dropoff),
    page: input.page ?? 1,
    limit: input.limit ?? 100,
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
  anioFabricacion?: number;
  capacidadPasajeros?: number;
  capacidadMaletas?: number;
  aireAcondicionado?: boolean;
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
  const { fechaRecogida, fechaDevolucion } = defaultCatalogApiDateRange();
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

/** Carga flota completa en paralelo (web / catálogo público). */
export async function loadAllCatalogVehicles(
  localizaciones: { idLocalizacion: number }[],
  buscar: (params: Record<string, unknown>) => Promise<{ data?: unknown }>,
) {
  const { fechaRecogida, fechaDevolucion } = defaultCatalogApiDateRange();
  const results = await Promise.allSettled(
    localizaciones.map((loc) =>
      buscar({
        idLocalizacion: loc.idLocalizacion,
        fechaRecogida,
        fechaDevolucion,
        page: 1,
        limit: 100,
      }),
    ),
  );
  const byId = new Map<number, VehiculoBooking>();
  results.forEach((res) => {
    if (res.status !== 'fulfilled') return;
    const payload = getPayload<{ vehiculos?: Record<string, unknown>[] }>(res.value);
    (payload.vehiculos ?? []).forEach((v) => {
      const norm = normalizeVehiculoFromBookingList(v);
      const key = norm.idVehiculo ?? norm.id;
      if (key != null && !byId.has(key)) byId.set(key, norm);
    });
  });
  return Array.from(byId.values());
}
