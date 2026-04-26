import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import { authApi } from '../../api/authApi';
import { Car, Eye, EyeOff, Loader2 } from 'lucide-react';

export default function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const login = useAuthStore((s) => s.login);
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from?.pathname || '/dashboard';

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await authApi.login({ username, password });
      const data = response.data.data;
      login(data);
      navigate(from, { replace: true });
    } catch (err) {
      const msg = err.response?.data?.message || 'Error al iniciar sesión';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-bg">
        <div className="login-bg__gradient" />
      </div>
      <div className="login-card">
        <div className="login-card__header">
          <div className="login-card__logo">
            <Car size={36} />
          </div>
          <h1 className="login-card__title">Europcar Rental</h1>
          <p className="login-card__subtitle">Sistema de gestión de renta de vehículos</p>
        </div>

        <form className="login-form" onSubmit={handleSubmit}>
          {error && (
            <div className="login-form__error">
              {error}
            </div>
          )}

          <div className="form-group">
            <label htmlFor="username" className="form-label">Usuario</label>
            <input
              id="username"
              type="text"
              className="form-input"
              placeholder="Ingrese su usuario"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              autoFocus
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="password" className="form-label">Contraseña</label>
            <div className="form-input-wrapper">
              <input
                id="password"
                type={showPassword ? 'text' : 'password'}
                className="form-input"
                placeholder="Ingrese su contraseña"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                disabled={loading}
              />
              <button
                type="button"
                className="form-input-toggle"
                onClick={() => setShowPassword(!showPassword)}
                tabIndex={-1}
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
          </div>

          <button
            type="submit"
            className="btn btn--primary btn--full"
            disabled={loading || !username || !password}
          >
            {loading ? (
              <>
                <Loader2 size={18} className="spin" />
                Iniciando sesión...
              </>
            ) : (
              'Iniciar sesión'
            )}
          </button>
        </form>

        <div className="login-card__footer">
          <p>API: <code>{import.meta.env.VITE_API_URL}</code></p>
        </div>
      </div>
    </div>
  );
}
