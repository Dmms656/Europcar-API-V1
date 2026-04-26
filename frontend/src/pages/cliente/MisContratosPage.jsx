import { FileText, Clock, Car, DollarSign, Eye } from 'lucide-react';

const contratosMock = [
  { id: 1, numero: 'CTR-2026-001', vehiculo: 'Toyota Yaris', fechaSalida: '2026-04-10T09:00', fechaDevolucion: '2026-04-12T09:00', estado: 'CERRADO', total: 115.20 },
  { id: 2, numero: 'CTR-2026-002', vehiculo: 'Suzuki Grand Vitara', fechaSalida: '2026-05-01T10:00', fechaDevolucion: '2026-05-05T10:00', estado: 'ABIERTO', total: 425.50 },
];

const estadoColors = {
  ABIERTO: 'var(--color-info)',
  CERRADO: 'var(--color-success)',
  ANULADO: 'var(--color-danger)',
};

export default function MisContratosPage() {
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
            <button className="btn btn--ghost btn--sm">
              <Eye size={16} /> Ver
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
