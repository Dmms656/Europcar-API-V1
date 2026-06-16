const HISTORICOS = new Set(['CANCELADA', 'FINALIZADA']);

const getEstado = (r: Record<string, unknown>) =>
  String(r?.estadoReserva || r?.estado || '');

const getFechaDevolucion = (r: Record<string, unknown>) =>
  r?.fechaHoraDevolucion || r?.fechaDevolucion;

const getFechaRecogida = (r: Record<string, unknown>) =>
  r?.fechaHoraRecogida || r?.fechaRecogida;

export function isReservaHistorica(r: Record<string, unknown>) {
  if (!r) return false;
  if (HISTORICOS.has(getEstado(r))) return true;
  const fecha = getFechaDevolucion(r);
  if (!fecha) return false;
  return new Date(String(fecha)).getTime() <= Date.now();
}

export function isReservaActiva(r: Record<string, unknown>) {
  return !isReservaHistorica(r);
}

export function isReservaCancelable(r: Record<string, unknown>) {
  const estado = getEstado(r);
  if (estado !== 'PENDIENTE' && estado !== 'CONFIRMADA') return false;
  const recogida = getFechaRecogida(r);
  if (!recogida) return false;
  return new Date(String(recogida)).getTime() > Date.now();
}

export type ReservaItem = {
  idReserva?: number;
  id?: number;
  codigoReserva?: string;
  estadoReserva?: string;
  estado?: string;
  fechaHoraRecogida?: string;
  fechaRecogida?: string;
  fechaHoraDevolucion?: string;
  fechaDevolucion?: string;
  total?: number;
  vehiculo?: { marca?: string; modelo?: string };
  marcaVehiculo?: string;
  modeloVehiculo?: string;
};
