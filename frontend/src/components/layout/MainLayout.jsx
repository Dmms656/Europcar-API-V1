import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import { useAppStore } from '../../store/useAppStore';
import {
  LayoutDashboard, Users, Car, CalendarCheck, FileText,
  CreditCard, Wrench, LogOut, ChevronRight, Shield, MapPin
} from 'lucide-react';

const navigation = [
  { name: 'Dashboard', path: '/dashboard', icon: LayoutDashboard, roles: [] },
  { name: 'Clientes', path: '/clientes', icon: Users, roles: ['ADMIN', 'AGENTE_POS'] },
  { name: 'Vehículos', path: '/vehiculos', icon: Car, roles: [] },
  { name: 'Reservas', path: '/reservas', icon: CalendarCheck, roles: [] },
  { name: 'Contratos', path: '/contratos', icon: FileText, roles: ['ADMIN', 'AGENTE_POS'] },
  { name: 'Pagos', path: '/pagos', icon: CreditCard, roles: ['ADMIN', 'AGENTE_POS'] },
  { name: 'Mantenimientos', path: '/mantenimientos', icon: Wrench, roles: ['ADMIN', 'AGENTE_POS'] },
  { name: 'Localizaciones', path: '/localizaciones', icon: MapPin, roles: ['ADMIN', 'AGENTE_POS'] },
  { name: 'Usuarios', path: '/usuarios', icon: Shield, roles: ['ADMIN'] },
];

export default function MainLayout() {
  const { user, logout, hasAnyRole } = useAuthStore();
  const { sidebarOpen, setSidebarOpen } = useAppStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const visibleNavigation = navigation.filter(
    (item) => item.roles.length === 0 || hasAnyRole(...item.roles)
  );

  return (
    <div className="app-layout">
      {/* Mobile overlay */}
      {sidebarOpen && (
        <div className="sidebar-overlay" onClick={() => setSidebarOpen(false)} />
      )}

      {/* Sidebar */}
      <aside className={`sidebar ${sidebarOpen ? 'sidebar--open' : ''}`}>
        <nav className="sidebar__nav">
          {visibleNavigation.map((item) => (
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
            <div className="sidebar__user-avatar">
              {user?.username?.charAt(0).toUpperCase()}
            </div>
            <div className="sidebar__user-info">
              <span className="sidebar__user-name">{user?.username}</span>
              <span className="sidebar__user-role">
                {user?.roles?.[0] || 'Usuario'}
              </span>
            </div>
          </div>
          <button className="sidebar__logout" onClick={handleLogout}>
            <LogOut size={18} />
            <span>Cerrar sesión</span>
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="main-content">
        <div className="page-content" style={{ marginTop: 'var(--spacing-md)' }}>
          <Outlet />
        </div>
      </main>
    </div>
  );
}
