import { useState, useEffect } from 'react';
import { FileText, Clock, Car, DollarSign, Eye, X, Hash, MapPin, User, Calendar, Loader2, Inbox } from 'lucide-react';
import { contratosApi } from '../../api/contratosApi';
import { useAuthStore } from '../../store/useAuthStore';

const estadoColors = {
  ABIERTO: 'var(--color-info)',
  CERRADO: 'var(--color-success)',
  ANULADO: 'var(--color-danger)',
};

export default function MisContratosPage() {
  const { user } = useAuthStore();
  const [contratos, setContratos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState(null);

  useEffect(() => {
    loadContratos();
  }, []);

  const loadContratos = async () => {
    setLoading(true);
    try {
      const res = await contratosApi.getAll();
      const data = res.data?.data;
      const all = Array.isArray(data) ? data : [];
      // Filter by client if idCliente is available
      const idCliente = user?.idCliente;
      const filtered = idCliente ? all.filter(c => c.idCliente === idCliente) : all;
      setContratos(filtered);
    } catch (err) {
      console.warn('Error cargando contratos:', err);
      setContratos([]);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="mis-contratos-page" style={{ display: 'flex', justifyContent: 'center', padding: '4rem' }}>
        <Loader2 size={32} className="spin" />
      </div>
    );
  }

  return (
    <div className="mis-contratos-page">
      <div className="page-header">
        <h1><FileText size={28} /> Mis Contratos</h1>
        <p className="page-subtitle">Contratos de arrendamiento activos y cerrados</p>
      </div>

      {contratos.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '3rem 1rem', opacity: 0.6 }}>
          <Inbox size={48} />
          <p style={{ marginTop: '1rem', fontSize: '1.1rem' }}>No tienes contratos registrados aún.</p>
        </div>
      ) : (
        <div className="contratos-list">
          {contratos.map((c) => (
            <div key={c.idContrato || c.id} className="contrato-item">
              <div className="contrato-item__icon">
                <FileText size={24} />
              </div>
              <div className="contrato-item__info">
                <div className="contrato-item__header">
                  <h3>{c.codigoContrato || c.numero || `CTR-${c.idContrato}`}</h3>
                  <span className="contrato-item__badge" style={{ background: estadoColors[c.estadoContrato || c.estado] || 'var(--color-border)' }}>
                    {c.estadoContrato || c.estado}
                  </span>
                </div>
                <div className="contrato-item__meta">
                  <span><Car size={14} /> {c.placaVehiculo || c.vehiculo || '—'}</span>
                  <span><Clock size={14} /> {new Date(c.fechaHoraSalida || c.fechaSalida).toLocaleDateString('es-EC')} — {new Date(c.fechaHoraDevolucion || c.fechaDevolucion).toLocaleDateString('es-EC')}</span>
                </div>
                <div className="contrato-item__footer">
                  <span className="contrato-item__total"><DollarSign size={14} /> ${(c.totalContrato || c.total || 0).toFixed(2)}</span>
                </div>
              </div>
              <button className="btn btn--ghost btn--sm" onClick={() => setSelected(c)}>
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
              <h2>Detalle de Contrato</h2>
              <button className="btn btn--ghost btn--sm" onClick={() => setSelected(null)}><X size={18} /></button>
            </div>
            <div className="modal__body">
              <div className="detail-badge-row">
                <span className="reserva-item__badge" style={{ background: estadoColors[selected.estadoContrato || selected.estado] }}>
                  {selected.estadoContrato || selected.estado}
                </span>
                <span className="detail-code"><Hash size={14} /> {selected.codigoContrato || selected.numero || `CTR-${selected.idContrato}`}</span>
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
                  <span className="detail-label"><Calendar size={14} /> Fecha Salida</span>
                  <span className="detail-value">{new Date(selected.fechaHoraSalida || selected.fechaSalida).toLocaleString('es-EC', { dateStyle: 'medium', timeStyle: 'short' })}</span>
                </div>
                <div className="detail-item">
                  <span className="detail-label"><Calendar size={14} /> Fecha Devolución</span>
                  <span className="detail-value">{new Date(selected.fechaHoraDevolucion || selected.fechaDevolucion).toLocaleString('es-EC', { dateStyle: 'medium', timeStyle: 'short' })}</span>
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
                  <span className="detail-value">${(selected.depositoGarantia || selected.deposito || 0).toFixed(2)}</span>
                </div>
              </div>

              <div className="detail-total">
                <span>Total del Contrato</span>
                <span className="detail-total__amount"><DollarSign size={16} />{(selected.totalContrato || selected.total || 0).toFixed(2)}</span>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
