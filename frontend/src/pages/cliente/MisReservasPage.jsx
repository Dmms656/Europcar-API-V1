import { useState, useEffect } from 'react';
import { CalendarCheck, Clock, MapPin, Car, Eye, X, DollarSign, Hash, User, FileText, Loader2, Inbox } from 'lucide-react';
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
                  <h3>{r.placaVehiculo || r.vehiculo || 'Vehículo'}</h3>
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
              <button className="btn btn--ghost btn--sm" onClick={() => setSelected(r)}>
                <Eye size={16} /> Ver
              </button>
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
                  <span className="detail-value">{selected.placaVehiculo || selected.vehiculo || '—'}</span>
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
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
