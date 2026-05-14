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

function sucursalNombrePlano(localizacion) {
  if (localizacion == null) return '';
  if (typeof localizacion === 'string') return localizacion;
  return (
    localizacion.nombre
    || localizacion.nombreLocalizacion
    || ''
  ).toString();
}

/** Item de lista GET /vehiculos (middleware / monolito booking). */
export function normalizeVehiculoFromBookingList(v) {
  if (!v) return v;
  const loc = v.localizacion;
  const idLoc =
    v.idLocalizacion
    ?? (typeof loc === 'object' && loc ? (loc.idLocalizacion ?? loc.id) : null);
  const marcaStr =
    typeof v.marca === 'object' && v.marca?.nombre != null ? v.marca.nombre : (v.marca || '');
  const catStr =
    typeof v.categoria === 'object' && v.categoria?.nombre != null
      ? v.categoria.nombre
      : (v.categoria || v.nombreCategoria || '');

  return {
    ...v,
    marca: marcaStr || v.marca,
    categoria: catStr || v.categoria,
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

/** Detalle GET /vehiculos/:id */
export function normalizeVehiculoDetalle(v) {
  if (!v) return null;
  const marca =
    typeof v.marca === 'object' && v.marca?.nombre != null ? v.marca.nombre : (v.marca || '');
  const categoria =
    typeof v.categoria === 'object' && v.categoria?.nombre != null
      ? v.categoria.nombre
      : (v.categoria || '');
  const loc = v.localizacion;
  const idLoc =
    v.idLocalizacion
    ?? (typeof loc === 'object' && loc ? (loc.idLocalizacion ?? loc.id) : null);
  const precioBase =
    v.precioBaseDia
    ?? v.precioDia
    ?? (typeof v.precio === 'object' && v.precio ? v.precio.precioBaseDia : undefined)
    ?? 0;

  return {
    ...v,
    marca,
    categoria,
    modelo: v.modelo || v.modeloVehiculo,
    anioFabricacion: v.anioFabricacion ?? v.anio,
    tipoCombustible: v.tipoCombustible || v.combustible,
    tipoTransmision: v.tipoTransmision || v.transmision,
    idLocalizacion: idLoc,
    precioBaseDia: precioBase,
    precioDia: v.precioDia ?? precioBase,
  };
}
