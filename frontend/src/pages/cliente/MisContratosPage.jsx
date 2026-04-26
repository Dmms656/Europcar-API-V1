import { useState } from 'react';
import { FileText, Clock, Car, DollarSign, Eye, X, Hash, MapPin, User, Calendar } from 'lucide-react';

const contratosMock = [
  { id: 1, numero: 'CTR-2026-001', vehiculo: 'Toyota Yaris', placa: 'ABC-5678', categoria: 'Sedán', conductor: 'Juan Pérez', fechaSalida: '2026-04-10T09:00', fechaDevolucion: '2026-04-12T09:00', sucursal: 'Quito Centro', estado: 'CERRADO', total: 115.20, deposito: 200.00, kmSalida: 45230, kmEntrega: 45580 },
  { id: 2, numero: 'CTR-2026-002', vehiculo: 'Suzuki Grand Vitara', placa: 'PBX-1234', categoria: 'SUV', conductor: 'Juan Pérez', fechaSalida: '2026-05-01T10:00', fechaDevolucion: '2026-05-05T10:00', sucursal: 'Aeropuerto Quito', estado: 'ABIERTO', total: 425.50, deposito: 500.00, kmSalida: 12000, kmEntrega: null },
];

const estadoColors = {
  ABIERTO: 'var(--color-info)',
  CERRADO: 'var(--color-success)',
  ANULADO: 'var(--color-danger)',
};

export default function MisContratosPage() {
  const [selected, setSelected] = useState(null);

  return (
    <div className="mis-contratos-page">
      <div className="page-header">
        <h1><FileText size={28} /> Mis Contratos</h1>
        <p className="page-subtitle">Contratos de arrendamiento activos y cerrados</p>
      </div>

      <div className="contratos-list">
        {contratosMock.map((c) => (
          <div key={c.id} className="contrato-item">
            <div className="contrato-item__icon">
              <FileText size={24} />
            </div>
            <div className="contrato-item__info">
              <div className="contrato-item__header">
                <h3>{c.numero}</h3>
                <span className="contrato-item__badge" style={{ background: estadoColors[c.estado] }}>
                  {c.estado}
                </span>
              </div>
              <div className="contrato-item__meta">
                <span><Car size={14} /> {c.vehiculo}</span>
                <span><Clock size={14} /> {new Date(c.fechaSalida).toLocaleDateString('es-EC')} — {new Date(c.fechaDevolucion).toLocaleDateString('es-EC')}</span>
              </div>
              <div className="contrato-item__footer">
                <span className="contrato-item__total"><DollarSign size={14} /> ${c.total.toFixed(2)}</span>
              </div>
            </div>
            <button className="btn btn--ghost btn--sm" onClick={() => setSelected(c)}>
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
              <h2>Detalle de Contrato</h2>
              <button className="btn btn--ghost btn--sm" onClick={() => setSelected(null)}><X size={18} /></button>
            </div>
            <div className="modal__body">
              <div className="detail-badge-row">
                <span className="reserva-item__badge" style={{ background: estadoColors[selected.estado] }}>
                  {selected.estado}
                </span>
                <span className="detail-code"><Hash size={14} /> {selected.numero}</span>
              </div>

              <div className="detail-grid">
                <div className="detail-item">
                  <span className="detail-label"><Car size={14} /> Vehículo</span>
                  <span className="detail-value">{selected.vehiculo}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label">Placa</span>
                  <span className="detail-value">{selected.placa}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label">Categoría</span>
                  <span className="detail-value">{selected.categoria}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label"><User size={14} /> Conductor</span>
                  <span className="detail-value">{selected.conductor}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label"><Calendar size={14} /> Fecha Salida</span>
                  <span className="detail-value">{new Date(selected.fechaSalida).toLocaleString('es-EC', { dateStyle: 'medium', timeStyle: 'short' })}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label"><Calendar size={14} /> Fecha Devolución</span>
                  <span className="detail-value">{new Date(selected.fechaDevolucion).toLocaleString('es-EC', { dateStyle: 'medium', timeStyle: 'short' })}</span>
                </div>
                <div className="detail-item detail-item--full">
                  <span className="detail-label"><MapPin size={14} /> Sucursal</span>
                  <span className="detail-value">{selected.sucursal}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label">KM Salida</span>
                  <span className="detail-value">{selected.kmSalida?.toLocaleString() || '—'}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label">KM Entrega</span>
                  <span className="detail-value">{selected.kmEntrega?.toLocaleString() || 'Pendiente'}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label">Depósito</span>
                  <span className="detail-value">${selected.deposito?.toFixed(2)}</span>
                </div>
              </div>

              <div className="detail-total">
                <span>Total del Contrato</span>
                <span className="detail-total__amount"><DollarSign size={16} />{selected.total.toFixed(2)}</span>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
