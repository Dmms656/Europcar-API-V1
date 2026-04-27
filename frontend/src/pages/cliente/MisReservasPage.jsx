import { useState, useEffect } from 'react';
import { CalendarCheck, Clock, Car, Eye, X, DollarSign, Hash, User, FileText, Loader2, Inbox, XCircle, Lock } from 'lucide-react';
import { reservasApi } from '../../api/reservasApi';
import { useAuthStore } from '../../store/useAuthStore';
import { toast } from 'sonner';

const estadoColors = {
  PENDIENTE: 'var(--color-warning)',
  CONFIRMADA: 'var(--color-info)',
  EN_CURSO: 'var(--color-accent)',
  FINALIZADA: 'var(--color-success)',
  CANCELADA: 'var(--color-danger)',
};

export default function MisReservasPage() {
  const { user } = useAuthStore();
  const [reservas, setReservas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState(null);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelTarget, setCancelTarget] = useState(null);
  const [cancelMotivo, setCancelMotivo] = useState('');
  const [cancelling, setCancelling] = useState(false);

  useEffect(() => {
    loadReservas();
  }, []);

  const loadReservas = async () => {
    setLoading(true);
    try {
      const idCliente = user?.idCliente;
      if (!idCliente) { setLoading(false); return; }
      const res = await reservasApi.getByCliente(idCliente);
      const data = res.data?.data;
      setReservas(Array.isArray(data) ? data : []);
    } catch (err) {
      console.warn('Error cargando reservas:', err);
      setReservas([]);
    } finally {
      setLoading(false);
    }
  };

  const openCancelModal = (reserva) => {
    setCancelTarget(reserva);
    setCancelMotivo('');
    setShowCancelModal(true);
    setSelected(null); // close detail modal if open
  };

  const handleCancelar = async () => {
    if (!cancelMotivo.trim()) {
      toast.error('Ingresa el motivo de cancelación');
      return;
    }
    setCancelling(true);
    try {
      await reservasApi.cancelar(cancelTarget.idReserva, cancelMotivo.trim());
      toast.success('Reserva cancelada. Pagos y facturas asociados han sido anulados.');
      setShowCancelModal(false);
      setCancelTarget(null);
      loadReservas();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al cancelar la reserva');
    } finally { setCancelling(false); }
  };

  // Una reserva sólo se puede cancelar si:
  //  - su estado es PENDIENTE o CONFIRMADA, y
  //  - su fecha de recogida está en el futuro (las pasadas/en curso quedan lockeadas).
  const isFutura = (r) => {
    const fecha = r.fechaHoraRecogida || r.fechaRecogida;
    if (!fecha) return false;
    return new Date(fecha).getTime() > Date.now();
  };

  const canCancel = (r) => {
    const estado = r.estadoReserva || r.estado;
    const estadoOk = estado === 'PENDIENTE' || estado === 'CONFIRMADA';
    return estadoOk && isFutura(r);
  };

  const isLocked = (r) => {
    const estado = r.estadoReserva || r.estado;
    const estadoCancelable = estado === 'PENDIENTE' || estado === 'CONFIRMADA';
    return estadoCancelable && !isFutura(r);
  };

  if (loading) {
    return (
      <div className="mis-reservas-page" style={{ display: 'flex', justifyContent: 'center', padding: '4rem' }}>
        <Loader2 size={32} className="spin" />
      </div>
    );
  }

  return (
    <div className="mis-reservas-page">
      <div className="page-header">
        <h1><CalendarCheck size={28} /> Mis Reservas</h1>
        <p className="page-subtitle">Historial de todas tus reservas</p>
      </div>

      {reservas.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '3rem 1rem', opacity: 0.6 }}>
          <Inbox size={48} />
          <p style={{ marginTop: '1rem', fontSize: '1.1rem' }}>No tienes reservas registradas aún.</p>
        </div>
      ) : (
        <div className="reservas-list">
          {reservas.map((r) => (
            <div key={r.idReserva || r.id} className="reserva-item">
              <div className="reserva-item__icon">
                <Car size={24} />
              </div>
              <div className="reserva-item__info">
                <div className="reserva-item__header">
                  <h3>{r.descripcionVehiculo || r.vehiculo || 'Vehículo'} {r.placaVehiculo ? `(${r.placaVehiculo})` : ''}</h3>
                  <span className="reserva-item__badge" style={{ background: estadoColors[r.estadoReserva || r.estado] || 'var(--color-border)' }}>
                    {r.estadoReserva || r.estado}
                  </span>
                </div>
                <div className="reserva-item__meta">
                  <span><Clock size={14} /> {new Date(r.fechaHoraRecogida || r.fechaRecogida).toLocaleDateString('es-EC')} — {new Date(r.fechaHoraDevolucion || r.fechaDevolucion).toLocaleDateString('es-EC')}</span>
                </div>
                <div className="reserva-item__footer">
                  <span className="reserva-item__code">{r.codigoReserva || r.codigo}</span>
                  <span className="reserva-item__total">${(r.total || 0).toFixed(2)}</span>
                </div>
              </div>
              <div style={{display:'flex', gap:'0.5rem', alignItems:'center'}}>
                <button className="btn btn--ghost btn--sm" onClick={() => setSelected(r)}>
                  <Eye size={16} /> Ver
                </button>
                {canCancel(r) && (
                  <button className="btn btn--ghost btn--sm" style={{color:'var(--color-danger)'}} onClick={() => openCancelModal(r)}>
                    <XCircle size={16} /> Cancelar
                  </button>
                )}
                {isLocked(r) && (
                  <span
                    className="reserva-item__badge"
                    title="Esta reserva ya inició o pasó su fecha de recogida y no puede cancelarse desde el portal del cliente."
                    style={{ background: 'var(--color-border)', color: 'var(--color-text-secondary)', display:'inline-flex', alignItems:'center', gap:4 }}
                  >
                    <Lock size={12} /> Bloqueada
                  </span>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Detail Modal */}
      {selected && (
        <div className="modal-overlay" onClick={() => setSelected(null)}>
          <div className="modal detail-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal__header">
              <h2>Detalle de Reserva</h2>
              <button className="btn btn--ghost btn--sm" onClick={() => setSelected(null)}><X size={18} /></button>
            </div>
            <div className="modal__body">
              <div className="detail-badge-row">
                <span className="reserva-item__badge" style={{ background: estadoColors[selected.estadoReserva || selected.estado] }}>
                  {selected.estadoReserva || selected.estado}
                </span>
                <span className="detail-code"><Hash size={14} /> {selected.codigoReserva || selected.codigo}</span>
              </div>

              <div className="detail-grid">
                <div className="detail-item">
                  <span className="detail-label"><Car size={14} /> Vehículo</span>
                  <span className="detail-value">{selected.descripcionVehiculo || selected.vehiculo || '—'} {selected.placaVehiculo ? `• Placa: ${selected.placaVehiculo}` : ''}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label"><User size={14} /> Cliente</span>
                  <span className="detail-value">{selected.nombreCliente || user?.nombreCompleto || '—'}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label"><Clock size={14} /> Recogida</span>
                  <span className="detail-value">{new Date(selected.fechaHoraRecogida || selected.fechaRecogida).toLocaleString('es-EC', { dateStyle: 'medium', timeStyle: 'short' })}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label"><Clock size={14} /> Devolución</span>
                  <span className="detail-value">{new Date(selected.fechaHoraDevolucion || selected.fechaDevolucion).toLocaleString('es-EC', { dateStyle: 'medium', timeStyle: 'short' })}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label">Subtotal</span>
                  <span className="detail-value">${(selected.subtotal || 0).toFixed(2)}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label">Impuestos</span>
                  <span className="detail-value">${(selected.valorImpuestos || 0).toFixed(2)}</span>
                </div>
                {(selected.cargoOneWay || 0) > 0 && (
                  <div className="detail-item">
                    <span className="detail-label">Cargo One-Way</span>
                    <span className="detail-value">${selected.cargoOneWay.toFixed(2)}</span>
                  </div>
                )}
                {selected.extras && selected.extras.length > 0 && (
                  <div className="detail-item detail-item--full">
                    <span className="detail-label">Extras</span>
                    <div className="detail-extras">
                      {selected.extras.map((e, i) => <span key={i} className="detail-extra-tag">{e.nombreExtra || e} x{e.cantidad || 1}</span>)}
                    </div>
                  </div>
                )}
                {selected.codigoConfirmacion && (
                  <div className="detail-item detail-item--full">
                    <span className="detail-label"><FileText size={14} /> Código Confirmación</span>
                    <span className="detail-value">{selected.codigoConfirmacion}</span>
                  </div>
                )}
              </div>

              <div className="detail-total">
                <span>Total</span>
                <span className="detail-total__amount"><DollarSign size={16} />{(selected.total || 0).toFixed(2)}</span>
              </div>

              {canCancel(selected) && (
                <div style={{marginTop:'1rem', textAlign:'center'}}>
                  <button className="btn btn--outline" style={{borderColor:'var(--color-danger)', color:'var(--color-danger)'}} onClick={() => openCancelModal(selected)}>
                    <XCircle size={16} /> Cancelar esta reserva
                  </button>
                </div>
              )}
              {isLocked(selected) && (
                <div style={{
                  marginTop: '1rem',
                  padding: '0.75rem 1rem',
                  background: 'rgba(148, 163, 184, 0.12)',
                  borderRadius: 'var(--radius-md)',
                  border: '1px dashed var(--color-border)',
                  color: 'var(--color-text-secondary)',
                  fontSize: '0.85rem',
                  display: 'flex',
                  gap: 8,
                  alignItems: 'center'
                }}>
                  <Lock size={14} />
                  Esta reserva ya inició o su fecha de recogida pasó. La cancelación queda bloqueada para el cliente; contacta a soporte si necesitas asistencia.
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Cancel Confirmation Modal */}
      {showCancelModal && cancelTarget && (
        <div className="modal-overlay" onClick={() => setShowCancelModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal__header">
              <h2>Cancelar Reserva</h2>
              <button className="icon-btn" onClick={() => setShowCancelModal(false)}><X size={18} /></button>
            </div>
            <div className="modal__body">
              <div style={{
                padding: '1rem',
                background: 'rgba(239, 68, 68, 0.1)',
                borderRadius: 'var(--radius-md)',
                border: '1px solid rgba(239, 68, 68, 0.3)',
                marginBottom: '1rem'
              }}>
                <p style={{color:'var(--color-danger)', fontWeight:600, marginBottom:'0.5rem'}}>⚠️ Esta acción no se puede deshacer</p>
                <p style={{fontSize:'0.85rem', color:'var(--color-text-secondary)'}}>
                  Al cancelar la reserva <strong>{cancelTarget.codigoReserva}</strong>, se anularán automáticamente
                  todos los pagos y facturas asociados.
                </p>
              </div>
              <div className="form-group">
                <label className="form-label">Motivo de cancelación *</label>
                <textarea className="form-input" rows={3} value={cancelMotivo}
                  onChange={(e) => setCancelMotivo(e.target.value)}
                  placeholder="Describe el motivo por el cual deseas cancelar tu reserva..."
                  style={{resize:'vertical'}} />
              </div>
            </div>
            <div className="modal__footer">
              <button className="btn btn--ghost" onClick={() => setShowCancelModal(false)}>Volver</button>
              <button className="btn" disabled={cancelling}
                style={{background:'var(--color-danger)', color:'white'}}
                onClick={handleCancelar}>
                {cancelling ? <><Loader2 size={16} className="spin" /> Cancelando...</> : 'Confirmar Cancelación'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
