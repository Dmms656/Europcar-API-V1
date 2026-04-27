/**
 * Clasificación de reservas para el portal del cliente.
 *
 *  - "Activa": el cliente puede usar / cancelar la reserva. Aún no se ha finalizado y la
 *    fecha de devolución es futura. Incluye PENDIENTE, CONFIRMADA y EN_CURSO con devolución
 *    aún por venir.
 *  - "Histórica": cualquier reserva que ya pasó (fecha de devolución vencida) o cuyo estado
 *    final es CANCELADA o FINALIZADA, sin importar la fecha.
 */

const HISTORICOS = new Set(['CANCELADA', 'FINALIZADA']);

const getEstado = (r) => r?.estadoReserva || r?.estado || '';
const getFechaDevolucion = (r) => r?.fechaHoraDevolucion || r?.fechaDevolucion;
const getFechaRecogida = (r) => r?.fechaHoraRecogida || r?.fechaRecogida;

export function isReservaHistorica(r) {
  if (!r) return false;
  if (HISTORICOS.has(getEstado(r))) return true;
  const fecha = getFechaDevolucion(r);
  if (!fecha) return false;
  return new Date(fecha).getTime() <= Date.now();
}

export function isReservaActiva(r) {
  return !isReservaHistorica(r);
}

/**
 * ¿La reserva todavía es cancelable por el propio cliente?
 * Sólo PENDIENTE/CONFIRMADA cuya recogida aún no inició.
 */
export function isReservaCancelable(r) {
  if (!r) return false;
  const estado = getEstado(r);
  if (estado !== 'PENDIENTE' && estado !== 'CONFIRMADA') return false;
  const recogida = getFechaRecogida(r);
  if (!recogida) return false;
  return new Date(recogida).getTime() > Date.now();
}

/**
 * Reservas con estado cancelable pero recogida ya iniciada → bloqueadas.
 */
export function isReservaBloqueada(r) {
  if (!r) return false;
  const estado = getEstado(r);
  if (estado !== 'PENDIENTE' && estado !== 'CONFIRMADA') return false;
  const recogida = getFechaRecogida(r);
  if (!recogida) return false;
  return new Date(recogida).getTime() <= Date.now();
}
