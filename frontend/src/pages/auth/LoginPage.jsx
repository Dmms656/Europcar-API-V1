import { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import { authApi } from '../../api/authApi';
import { Car, Eye, EyeOff, Loader2, Shield, User, AlertCircle } from 'lucide-react';

export default function LoginPage() {
  const [tab, setTab] = useState('admin');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [touched, setTouched] = useState({});
  const [shake, setShake] = useState(false);

  const login = useAuthStore((s) => s.login);
  const navigate = useNavigate();
  const location = useLocation();

  const fieldErrors = {
    username: touched.username && !username.trim() ? 'El usuario es requerido' : '',
    password: touched.password && !password ? 'La contraseña es requerida'
      : touched.password && password.length < 3 ? 'Mínimo 3 caracteres' : '',
  };

  const handleBlur = (field) => setTouched(p => ({ ...p, [field]: true }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    setTouched({ username: true, password: true });
    setError('');

    if (!username.trim() || !password) {
      setShake(true);
      setTimeout(() => setShake(false), 500);
      return;
    }

    setLoading(true);
    try {
      const response = await authApi.login({ username, password });
      const data = response.data.data;
      const isAdmin = data.roles?.some(r => ['ADMIN', 'AGENTE_POS'].includes(r));

      if (tab === 'admin' && !isAdmin) {
        setError('Este usuario no tiene permisos de administración');
        setLoading(false);
        return;
      }

      const userType = isAdmin && tab === 'admin' ? 'admin' : 'cliente';
      login(data, userType);

      const from = location.state?.from?.pathname;
      if (from) {
        navigate(from, { replace: true });
      } else {
        navigate(userType === 'admin' ? '/dashboard' : '/mi-cuenta', { replace: true });
      }
    } catch (err) {
      const msg = err.response?.data?.message || 'Error al iniciar sesión';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-bg"><div className="login-bg__gradient" /></div>
      <div className={`login-card ${shake ? 'form-shake' : ''}`}>
        <div className="login-card__header">
          <div className="login-card__logo"><Car size={36} /></div>
          <h1 className="login-card__title">Europcar Rental</h1>
          <p className="login-card__subtitle">Accede a tu cuenta</p>
        </div>

        <div className="login-tabs">
          <button type="button" className={`login-tab ${tab === 'admin' ? 'login-tab--active' : ''}`}
            onClick={() => { setTab('admin'); setError(''); }}>
            <Shield size={18} /><span>Administrador</span>
          </button>
          <button type="button" className={`login-tab ${tab === 'cliente' ? 'login-tab--active' : ''}`}
            onClick={() => { setTab('cliente'); setError(''); }}>
            <User size={18} /><span>Cliente</span>
          </button>
        </div>

        <form className="login-form" onSubmit={handleSubmit} noValidate>
          {error && <div className="login-form__error">{error}</div>}

          <div className={`form-group ${fieldErrors.username ? 'form-group--error' : ''}`}>
            <label htmlFor="username" className="form-label">
              {tab === 'admin' ? 'Usuario' : 'Usuario / Correo'}
            </label>
            <input id="username" type="text" className="form-input"
              placeholder={tab === 'admin' ? 'admin.dev' : 'cliente.web'}
              value={username} onChange={(e) => setUsername(e.target.value)}
              onBlur={() => handleBlur('username')} autoFocus disabled={loading} />
            {fieldErrors.username && <span className="form-error"><AlertCircle size={13} /> {fieldErrors.username}</span>}
          </div>

          <div className={`form-group ${fieldErrors.password ? 'form-group--error' : ''}`}>
            <label htmlFor="password" className="form-label">Contraseña</label>
            <div className="form-input-wrapper">
              <input id="password" type={showPassword ? 'text' : 'password'} className="form-input"
                placeholder="Ingrese su contraseña" value={password}
                onChange={(e) => setPassword(e.target.value)}
                onBlur={() => handleBlur('password')} disabled={loading} />
              <button type="button" className="form-input-toggle"
                onClick={() => setShowPassword(!showPassword)} tabIndex={-1}>
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {fieldErrors.password && <span className="form-error"><AlertCircle size={13} /> {fieldErrors.password}</span>}
          </div>

          <button type="submit" className="btn btn--primary btn--full" disabled={loading}>
            {loading ? (
              <><Loader2 size={18} className="spin" /> Iniciando sesión...</>
            ) : (
              tab === 'admin' ? 'Acceder al Panel' : 'Acceder a Mi Cuenta'
            )}
          </button>
        </form>

        <div className="login-card__footer register-footer">
          {tab === 'cliente' && (
            <p>¿No tienes cuenta? <Link to="/registro" className="register-link">Regístrate aquí</Link></p>
          )}
          <Link to="/" className="login-card__back">← Volver al inicio</Link>
        </div>
      </div>
    </div>
  );
}
