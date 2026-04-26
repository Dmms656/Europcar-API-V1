import { CalendarCheck, Clock, MapPin, Car, Eye } from 'lucide-react';

// Simulated data (would come from API in production)
const reservasMock = [
  { id: 1, codigo: 'RES-A1B2C3', vehiculo: 'Suzuki Grand Vitara', fechaRecogida: '2026-05-01T10:00', fechaDevolucion: '2026-05-05T10:00', sucursal: 'Aeropuerto Quito', estado: 'CONFIRMADA', total: 425.50 },
  { id: 2, codigo: 'RES-D4E5F6', vehiculo: 'Toyota Yaris', fechaRecogida: '2026-04-10T09:00', fechaDevolucion: '2026-04-12T09:00', sucursal: 'Quito Centro', estado: 'FINALIZADA', total: 115.20 },
  { id: 3, codigo: 'RES-G7H8I9', vehiculo: 'Chevrolet Onix', fechaRecogida: '2026-03-15T14:00', fechaDevolucion: '2026-03-18T14:00', sucursal: 'Aeropuerto Guayaquil', estado: 'CANCELADA', total: 162.00 },
];

const estadoColors = {
  PENDIENTE: 'var(--color-warning)',
  CONFIRMADA: 'var(--color-info)',
  EN_CURSO: 'var(--color-accent)',
  FINALIZADA: 'var(--color-success)',
  CANCELADA: 'var(--color-danger)',
};

export default function MisReservasPage() {
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
            <button className="btn btn--ghost btn--sm">
              <Eye size={16} /> Ver
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
