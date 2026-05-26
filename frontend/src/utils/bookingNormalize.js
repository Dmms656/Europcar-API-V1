/**
 * Utilidades para alinear el SPA con la API booking (/api/v1) del middleware RedCar.
 * El contrato devuelve combustible/transmisión y localización anidada; el UI legacy
 * esperaba tipoCombustible / tipoTransmision y nombres planos.
 */

/** Formato datetime-local (sin zona): YYYY-MM-DDTHH:mm */
export function toDateTimeLocalValue(d) {
  const pad = (n) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

/** Ventana por defecto: mañana 10:00 → +2 días 10:00 (mínimo rango típico de alquiler). */
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

/** Extrae texto de referencias anidadas (camelCase o PascalCase del middleware .NET). */
function refNombre(ref) {
  if (ref == null) return '';
  if (typeof ref === 'string') return ref;
  if (typeof ref === 'object') {
    return (
      ref.nombre
      ?? ref.Nombre
      ?? ref.nombreMarca
      ?? ref.NombreMarca
      ?? ref.nombreCategoria
      ?? ref.NombreCategoria
      ?? ''
    ).toString();
  }
  return String(ref);
}

function refId(ref, ...keys) {
  if (!ref || typeof ref !== 'object') return null;
  for (const k of keys) {
    if (ref[k] != null) return ref[k];
  }
  return null;
}

function sucursalNombrePlano(localizacion) {
  if (localizacion == null) return '';
  if (typeof localizacion === 'string') return localizacion;
  return (
    localizacion.nombre
    ?? localizacion.Nombre
    ?? localizacion.nombreLocalizacion
    ?? localizacion.NombreLocalizacion
    ?? ''
  ).toString();
}

/** Item de lista GET /vehiculos (middleware / monolito booking). */
export function normalizeVehiculoFromBookingList(v) {
  if (!v) return v;
  const loc = v.localizacion;
  const idLoc =
    v.idLocalizacion
    ?? (typeof loc === 'object' && loc
      ? (refId(loc, 'idLocalizacion', 'IdLocalizacion', 'id', 'Id'))
      : null);
  const marcaStr = refNombre(v.marca);
  const catStr = refNombre(v.categoria) || v.nombreCategoria || v.NombreCategoria || '';

  return {
    ...v,
    marca: marcaStr,
    categoria: catStr,
    modelo: v.modelo || v.modeloVehiculo,
    anioFabricacion: v.anioFabricacion ?? v.anio,
    tipoCombustible: v.tipoCombustible || v.combustible,
    tipoTransmision: v.tipoTransmision || v.transmision,
    /** Nombre sucursal para filtros del catálogo (mapa por nombre). */
    nombreSucursal: sucursalNombrePlano(loc),
    /** Texto para la tarjeta (antes se usaba string plano del monolito). */
    localizacion: sucursalNombrePlano(loc) || v.localizacion,
    idLocalizacion: idLoc,
  };
}

/**
 * Rellena el paso «Datos del Cliente» desde el perfil de sesión (/Auth/me + MS Clientes).
 * Nunca usa username como nombre.
 */
export function guestFormFromUserProfile(user) {
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

  const cedula = (user.numeroIdentificacion || '').trim();
  const correo = (user.correo || '').trim();
  const telefono = (user.telefono || '').trim();

  return {
    nombre,
    apellido,
    cedula,
    correo,
    telefono,
    direccion: '',
  };
}

/**
 * Correo y teléfono válidos para POST /reservas (FluentValidation exige ambos).
 */
export function normalizeContactoReserva(user, guestForm = {}) {
  const correoRaw = (guestForm.correo || user?.correo || '').trim();
  const telefonoRaw = (guestForm.telefono || '').trim();
  const slug = String(user?.username || guestForm.cedula || 'cliente')
    .replace(/\W/g, '')
    .slice(0, 24) || 'cliente';
  const correo = correoRaw && correoRaw.includes('@')
    ? correoRaw
    : `${slug}@reserva.europcar.ec`;
  const telefono = telefonoRaw.length >= 7 ? telefonoRaw : '0999999999';
  return { correo, telefono };
}

/** Detalle GET /vehiculos/:id */
export function normalizeVehiculoDetalle(v) {
  if (!v) return null;
  const loc = v.localizacion;
  const precioObj = v.precio;
  const precioBase =
    v.precioBaseDia
    ?? v.precioDia
    ?? (precioObj && (precioObj.precioBaseDia ?? precioObj.PrecioBaseDia))
    ?? 0;

  return {
    ...v,
    marca: refNombre(v.marca),
    categoria: refNombre(v.categoria),
    modelo: v.modelo || v.modeloVehiculo,
    anioFabricacion: v.anioFabricacion ?? v.anio,
    tipoCombustible: v.tipoCombustible || v.combustible || v.Combustible,
    tipoTransmision: v.tipoTransmision || v.transmision || v.Transmision,
    imagenUrl: v.imagenUrl ?? v.ImagenUrl,
    idLocalizacion:
      v.idLocalizacion
      ?? (typeof loc === 'object' && loc
        ? refId(loc, 'idLocalizacion', 'IdLocalizacion', 'id', 'Id')
        : null),
    localizacion: sucursalNombrePlano(loc) || v.localizacion,
    precioBaseDia: precioBase,
    precioDia: v.precioDia ?? precioBase,
  };
}
