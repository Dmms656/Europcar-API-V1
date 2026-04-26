import { useState } from 'react';
import { CalendarCheck, Clock, MapPin, Car, Eye, X, DollarSign, Hash, User, FileText } from 'lucide-react';

const reservasMock = [
  { id: 1, codigo: 'RES-A1B2C3', vehiculo: 'Suzuki Grand Vitara', categoria: 'SUV', placa: 'PBX-1234', fechaRecogida: '2026-05-01T10:00', fechaDevolucion: '2026-05-05T10:00', sucursal: 'Aeropuerto Quito', estado: 'CONFIRMADA', total: 425.50, extras: ['GPS', 'Silla para bebé'], conductor: 'Juan Pérez' },
  { id: 2, codigo: 'RES-D4E5F6', vehiculo: 'Toyota Yaris', categoria: 'Sedán', placa: 'ABC-5678', fechaRecogida: '2026-04-10T09:00', fechaDevolucion: '2026-04-12T09:00', sucursal: 'Quito Centro', estado: 'FINALIZADA', total: 115.20, extras: [], conductor: 'Juan Pérez' },
  { id: 3, codigo: 'RES-G7H8I9', vehiculo: 'Chevrolet Onix', categoria: 'Compacto', placa: 'XYZ-9012', fechaRecogida: '2026-03-15T14:00', fechaDevolucion: '2026-03-18T14:00', sucursal: 'Aeropuerto Guayaquil', estado: 'CANCELADA', total: 162.00, extras: ['Seguro Premium'], conductor: 'Juan Pérez' },
];

const estadoColors = {
  PENDIENTE: 'var(--color-warning)',
  CONFIRMADA: 'var(--color-info)',
  EN_CURSO: 'var(--color-accent)',
  FINALIZADA: 'var(--color-success)',
  CANCELADA: 'var(--color-danger)',
};

export default function MisReservasPage() {
  const [selected, setSelected] = useState(null);

  return (
    <div className="mis-reservas-page">
      <div className="page-header">
        <h1><CalendarCheck size={28} /> Mis Reservas</h1>
        <p className="page-subtitle">Historial de todas tus reservas</p>
      </div>

      <div className="reservas-list">
        {reservasMock.map((r) => (
          <div key={r.id} className="reserva-item">
            <div className="reserva-item__icon">
              <Car size={24} />
            </div>
            <div className="reserva-item__info">
              <div className="reserva-item__header">
                <h3>{r.vehiculo}</h3>
                <span className="reserva-item__badge" style={{ background: estadoColors[r.estado] || 'var(--color-border)' }}>
                  {r.estado}
                </span>
              </div>
              <div className="reserva-item__meta">
                <span><Clock size={14} /> {new Date(r.fechaRecogida).toLocaleDateString('es-EC')} — {new Date(r.fechaDevolucion).toLocaleDateString('es-EC')}</span>
                <span><MapPin size={14} /> {r.sucursal}</span>
              </div>
              <div className="reserva-item__footer">
                <span className="reserva-item__code">{r.codigo}</span>
                <span className="reserva-item__total">${r.total.toFixed(2)}</span>
              </div>
            </div>
            <button className="btn btn--ghost btn--sm" onClick={() => setSelected(r)}>
              <Eye size={16} /> Ver
            </button>
          </div>
        ))}
      </div>

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
                <span className="reserva-item__badge" style={{ background: estadoColors[selected.estado] }}>
                  {selected.estado}
                </span>
                <span className="detail-code"><Hash size={14} /> {selected.codigo}</span>
              </div>

              <div className="detail-grid">
                <div className="detail-item">
                  <span className="detail-label"><Car size={14} /> Vehículo</span>
                  <span className="detail-value">{selected.vehiculo}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label">Categoría</span>
                  <span className="detail-value">{selected.categoria}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label">Placa</span>
                  <span className="detail-value">{selected.placa}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label"><User size={14} /> Conductor</span>
                  <span className="detail-value">{selected.conductor}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label"><Clock size={14} /> Recogida</span>
                  <span className="detail-value">{new Date(selected.fechaRecogida).toLocaleString('es-EC', { dateStyle: 'medium', timeStyle: 'short' })}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label"><Clock size={14} /> Devolución</span>
                  <span className="detail-value">{new Date(selected.fechaDevolucion).toLocaleString('es-EC', { dateStyle: 'medium', timeStyle: 'short' })}</span>
                </div>
                <div className="detail-item detail-item--full">
                  <span className="detail-label"><MapPin size={14} /> Sucursal</span>
                  <span className="detail-value">{selected.sucursal}</span>
                </div>
                {selected.extras.length > 0 && (
                  <div className="detail-item detail-item--full">
                    <span className="detail-label">Extras</span>
                    <div className="detail-extras">
                      {selected.extras.map((e, i) => <span key={i} className="detail-extra-tag">{e}</span>)}
                    </div>
                  </div>
                )}
              </div>

              <div className="detail-total">
                <span>Total</span>
                <span className="detail-total__amount"><DollarSign size={16} />{selected.total.toFixed(2)}</span>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
