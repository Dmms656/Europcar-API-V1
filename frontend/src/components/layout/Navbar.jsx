import { NavLink, Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import { useAppStore } from '../../store/useAppStore';
import { Car, Home, ShoppingBag, User, Shield, LogOut, Menu, X } from 'lucide-react';
import { useState } from 'react';

export default function Navbar() {
  const { user, token, logout, hasAnyRole } = useAuthStore();
  const { sidebarOpen, toggleSidebar } = useAppStore();
  const navigate = useNavigate();
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);

  const isAdmin = hasAnyRole?.('ADMIN', 'AGENTE_POS');
  const isLoggedIn = !!token;

  // Check if we are in a route that has a sidebar
  const isDashboardRoute = [
    '/dashboard', '/clientes', '/vehiculos', '/reservas', '/contratos', '/pagos', '/mantenimientos', '/usuarios',
    '/mi-cuenta', '/mis-reservas', '/mis-contratos', '/historial'
  ].some(path => location.pathname.startsWith(path));

  const handleLogout = () => {
    logout();
    navigate('/');
    setMenuOpen(false);
  };

  return (
    <nav className="navbar">
      <div className="navbar__inner">
        {/* Logo */}
        <Link to="/" className="navbar__logo">
          <Car size={26} />
          <span>Europcar</span>
        </Link>

        {/* Desktop Links */}
        <div className="navbar__links">
          <NavLink to="/" end className={({ isActive }) => `navbar__link ${isActive ? 'navbar__link--active' : ''}`}>
            <Home size={18} />
            <span>Inicio</span>
          </NavLink>
          <NavLink to="/catalogo" className={({ isActive }) => `navbar__link ${isActive ? 'navbar__link--active' : ''}`}>
            <ShoppingBag size={18} />
            <span>Catálogo</span>
          </NavLink>
          {isLoggedIn && isAdmin && (
            <NavLink to="/dashboard" className={({ isActive }) => `navbar__link ${isActive ? 'navbar__link--active' : ''}`}>
              <Shield size={18} />
              <span>Admin</span>
            </NavLink>
          )}
          {isLoggedIn && !isAdmin && (
            <NavLink to="/mi-cuenta" className={({ isActive }) => `navbar__link ${isActive ? 'navbar__link--active' : ''}`}>
              <User size={18} />
              <span>Mi Cuenta</span>
            </NavLink>
          )}
        </div>

        {/* Right side */}
        <div className="navbar__right">
          {isLoggedIn ? (
            <div className="navbar__user-area">
              <span className="navbar__username">{user?.nombreCompleto || user?.username}</span>
              <button className="navbar__logout-btn" onClick={handleLogout} title="Cerrar sesión">
                <LogOut size={18} />
              </button>
            </div>
          ) : (
            <Link to="/login" className="navbar__login-btn">
              Iniciar Sesión
            </Link>
          )}
          {/* Menu button logic depends on route */}
          {isDashboardRoute ? (
            <button className="navbar__hamburger" onClick={toggleSidebar}>
              {sidebarOpen ? <X size={24} /> : <Menu size={24} />}
            </button>
          ) : (
            <button className="navbar__hamburger" onClick={() => setMenuOpen(!menuOpen)}>
              {menuOpen ? <X size={24} /> : <Menu size={24} />}
            </button>
          )}
        </div>
      </div>

      {/* Mobile Menu (Only shows if NOT in dashboard, because dashboard has sidebar) */}
      {menuOpen && !isDashboardRoute && (
        <div className="navbar__mobile">
          <NavLink to="/" end className="navbar__mobile-link" onClick={() => setMenuOpen(false)}>
            <Home size={18} /> Inicio
          </NavLink>
          <NavLink to="/catalogo" className="navbar__mobile-link" onClick={() => setMenuOpen(false)}>
            <ShoppingBag size={18} /> Catálogo
          </NavLink>
          {isLoggedIn && isAdmin && (
            <NavLink to="/dashboard" className="navbar__mobile-link" onClick={() => setMenuOpen(false)}>
              <Shield size={18} /> Admin
            </NavLink>
          )}
          {isLoggedIn && !isAdmin && (
            <NavLink to="/mi-cuenta" className="navbar__mobile-link" onClick={() => setMenuOpen(false)}>
              <User size={18} /> Mi Cuenta
            </NavLink>
          )}
          {isLoggedIn ? (
            <button className="navbar__mobile-link navbar__mobile-logout" onClick={handleLogout}>
              <LogOut size={18} /> Cerrar Sesión
            </button>
          ) : (
            <NavLink to="/login" className="navbar__mobile-link" onClick={() => setMenuOpen(false)}>
              <User size={18} /> Iniciar Sesión
            </NavLink>
          )}
        </div>
      )}
    </nav>
  );
}
