import { NavLink, Outlet, useNavigate, Link } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import {
  Car, User, CalendarCheck, FileText, Clock,
  LogOut, Menu, X, ChevronRight, ShoppingBag
} from 'lucide-react';
import { useState } from 'react';

const navigation = [
  { name: 'Mi Cuenta', path: '/mi-cuenta', icon: User },
  { name: 'Mis Reservas', path: '/mis-reservas', icon: CalendarCheck },
  { name: 'Mis Contratos', path: '/mis-contratos', icon: FileText },
  { name: 'Historial', path: '/historial', icon: Clock },
];

export default function ClienteLayout() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(false);

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
        <div className="sidebar__header">
          <Link to="/" className="sidebar__logo">
            <Car size={28} />
            <span className="sidebar__brand">Europcar</span>
          </Link>
          <button className="sidebar__close" onClick={() => setSidebarOpen(false)}>
            <X size={20} />
          </button>
        </div>

        <div className="sidebar__cta">
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
        <header className="topbar topbar--cliente">
          <button className="topbar__menu" onClick={() => setSidebarOpen(true)}>
            <Menu size={22} />
          </button>
          <div className="topbar__title">Mi Portal</div>
          <div className="topbar__actions">
            <Link to="/catalogo" className="topbar__cta">
              <ShoppingBag size={18} />
              <span>Catálogo</span>
            </Link>
            <span className="topbar__user-name">{user?.username}</span>
          </div>
        </header>
        <div className="page-content">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
