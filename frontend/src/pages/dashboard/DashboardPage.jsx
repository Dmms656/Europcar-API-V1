import { useAuthStore } from '../../store/useAuthStore';
import { Car, Users, CalendarCheck, FileText } from 'lucide-react';

export default function DashboardPage() {
  const { user } = useAuthStore();

  const cards = [
    { title: 'Vehículos', desc: 'Gestionar flota', icon: Car, path: '/vehiculos', color: 'var(--color-primary)' },
    { title: 'Clientes', desc: 'Administrar clientes', icon: Users, path: '/clientes', color: 'var(--color-success)' },
    { title: 'Reservas', desc: 'Reservas activas', icon: CalendarCheck, path: '/reservas', color: 'var(--color-warning)' },
    { title: 'Contratos', desc: 'Contratos de renta', icon: FileText, path: '/contratos', color: 'var(--color-info)' },
  ];

  return (
    <div className="dashboard">
      <div className="dashboard__welcome">
        <h1>Bienvenido, {user?.username}</h1>
        <p>Panel de administración de Europcar Rental</p>
      </div>

      <div className="dashboard__cards">
        {cards.map((card) => (
          <a key={card.path} href={card.path} className="dashboard-card">
            <div className="dashboard-card__icon" style={{ background: card.color }}>
              <card.icon size={24} color="white" />
            </div>
            <div className="dashboard-card__content">
              <h3>{card.title}</h3>
              <p>{card.desc}</p>
            </div>
          </a>
        ))}
      </div>

      <div className="dashboard__info">
        <div className="info-card">
          <h3>Información de sesión</h3>
          <div className="info-card__row">
            <span>Usuario</span>
            <strong>{user?.username}</strong>
          </div>
          <div className="info-card__row">
            <span>Correo</span>
            <strong>{user?.correo}</strong>
          </div>
          <div className="info-card__row">
            <span>Roles</span>
            <div className="info-card__badges">
              {user?.roles?.map((role) => (
                <span key={role} className="badge">{role}</span>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
