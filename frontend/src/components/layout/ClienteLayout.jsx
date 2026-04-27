import { NavLink, Outlet, useNavigate, Link } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import { useAppStore } from '../../store/useAppStore';
import {
  User, CalendarCheck, FileText, Clock, ReceiptText,
  LogOut, ChevronRight, ShoppingBag
} from 'lucide-react';

const navigation = [
  { name: 'Mi Cuenta', path: '/mi-cuenta', icon: User },
  { name: 'Mis Reservas', path: '/mis-reservas', icon: CalendarCheck },
  { name: 'Mis Contratos', path: '/mis-contratos', icon: FileText },
  { name: 'Mis Facturas', path: '/mis-facturas', icon: ReceiptText },
  { name: 'Historial', path: '/historial', icon: Clock },
];

export default function ClienteLayout() {
  const { user, logout } = useAuthStore();
  const { sidebarOpen, setSidebarOpen } = useAppStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <div className="app-layout">
      {sidebarOpen && (
        <div className="sidebar-overlay" onClick={() => setSidebarOpen(false)} />
      )}

      <aside className={`sidebar sidebar--cliente ${sidebarOpen ? 'sidebar--open' : ''}`}>
        <div className="sidebar__cta" style={{ marginTop: 'var(--spacing-md)' }}>
          <Link to="/catalogo" className="btn btn--accent btn--full" onClick={() => setSidebarOpen(false)}>
            <ShoppingBag size={18} />
            <span>Reservar Vehículo</span>
          </Link>
        </div>

        <nav className="sidebar__nav">
          {navigation.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `sidebar__link ${isActive ? 'sidebar__link--active' : ''}`
              }
              onClick={() => setSidebarOpen(false)}
            >
              <item.icon size={20} />
              <span>{item.name}</span>
              <ChevronRight size={16} className="sidebar__link-arrow" />
            </NavLink>
          ))}
        </nav>

        <div className="sidebar__footer">
          <div className="sidebar__user">
            <div className="sidebar__user-avatar sidebar__user-avatar--cliente">
              {user?.username?.charAt(0).toUpperCase()}
            </div>
            <div className="sidebar__user-info">
              <span className="sidebar__user-name">{user?.nombreCompleto || user?.username}</span>
              <span className="sidebar__user-role">Cliente</span>
            </div>
          </div>
          <button className="sidebar__logout" onClick={handleLogout}>
            <LogOut size={18} />
            <span>Cerrar sesión</span>
          </button>
        </div>
      </aside>

      <main className="main-content">
        <div className="page-content" style={{ marginTop: 'var(--spacing-md)' }}>
          <Outlet />
        </div>
      </main>
    </div>
  );
}
