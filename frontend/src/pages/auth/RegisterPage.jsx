import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authApi } from '../../api/authApi';
import { Car, UserPlus, Eye, EyeOff, Loader2, User, Mail, Phone, MapPin, CheckCircle2 } from 'lucide-react';
import { toast } from 'sonner';

export default function RegisterPage() {
  const navigate = useNavigate();
  const [mode, setMode] = useState('nuevo'); // 'nuevo' | 'existente'
  const [step, setStep] = useState(1); // 1: form, 2: success
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');

  const [form, setForm] = useState({
    // User fields
    username: '',
    correo: '',
    password: '',
    confirmPassword: '',
    // Client fields (only for 'nuevo')
    nombre: '',
    apellido: '',
    telefono: '',
    direccion: '',
    cedula: '',
    // For 'existente'
    idClienteExistente: '',
  });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (form.password !== form.confirmPassword) {
      setError('Las contraseñas no coinciden');
      return;
    }
    if (form.password.length < 6) {
      setError('La contraseña debe tener al menos 6 caracteres');
      return;
    }

    setLoading(true);
    try {
      const payload = {
        username: form.username,
        correo: form.correo,
        password: form.password,
      };

      if (mode === 'nuevo') {
        payload.nombre = form.nombre;
        payload.apellido = form.apellido;
        payload.cedula = form.cedula;
        payload.telefono = form.telefono;
        payload.direccion = form.direccion;
      }

      await authApi.register(payload);
      setStep(2);
      toast.success('¡Cuenta creada exitosamente!');
    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data?.title || 'Error al crear la cuenta';
      setError(msg);
      toast.error(msg);
    } finally {
      setLoading(false);
    }
  };

  const updateField = (field, value) => setForm(prev => ({ ...prev, [field]: value }));

  if (step === 2) {
    return (
      <div className="login-page">
        <div className="login-bg"><div className="login-bg__gradient" /></div>
        <div className="login-card register-card">
          <div className="register-success">
            <CheckCircle2 size={56} className="register-success__icon" />
            <h1>¡Cuenta Creada!</h1>
            <p>Tu cuenta ha sido registrada exitosamente.</p>
            <p className="register-success__user">
              Usuario: <strong>{form.username}</strong>
            </p>
            <div className="register-success__actions">
              <Link to="/login" className="btn btn--primary btn--full">
                Iniciar Sesión
              </Link>
              <Link to="/catalogo" className="btn btn--outline btn--full">
                Explorar Catálogo
              </Link>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="login-page">
      <div className="login-bg"><div className="login-bg__gradient" /></div>
      <div className="login-card register-card">
        <div className="login-card__header">
          <div className="login-card__logo"><UserPlus size={36} /></div>
          <h1 className="login-card__title">Crear Cuenta</h1>
          <p className="login-card__subtitle">Regístrate para reservar vehículos</p>
        </div>

        {/* Mode selector */}
        <div className="login-tabs">
          <button type="button"
            className={`login-tab ${mode === 'nuevo' ? 'login-tab--active' : ''}`}
            onClick={() => { setMode('nuevo'); setError(''); }}>
            <UserPlus size={16} />
            <span>Nuevo Cliente</span>
          </button>
          <button type="button"
            className={`login-tab ${mode === 'existente' ? 'login-tab--active' : ''}`}
            onClick={() => { setMode('existente'); setError(''); }}>
            <User size={16} />
            <span>Cliente Existente</span>
          </button>
        </div>

        <form className="login-form register-form" onSubmit={handleSubmit}>
          {error && <div className="login-form__error">{error}</div>}

          {mode === 'existente' && (
            <div className="register-info">
              <p>Si ya eres cliente de Europcar, ingresa tu número de cédula o ID de cliente para vincular tu cuenta.</p>
            </div>
          )}

          {/* Client fields for new clients */}
          {mode === 'nuevo' && (
            <>
              <div className="register-row">
                <div className="form-group">
                  <label className="form-label"><User size={14} /> Nombre *</label>
                  <input className="form-input" placeholder="Juan" value={form.nombre}
                    onChange={(e) => updateField('nombre', e.target.value)} required />
                </div>
                <div className="form-group">
                  <label className="form-label">Apellido *</label>
                  <input className="form-input" placeholder="Pérez" value={form.apellido}
                    onChange={(e) => updateField('apellido', e.target.value)} required />
                </div>
              </div>
              <div className="form-group">
                <label className="form-label">Cédula / Pasaporte *</label>
                <input className="form-input" placeholder="1712345678" value={form.cedula}
                  onChange={(e) => updateField('cedula', e.target.value)} required />
              </div>
              <div className="register-row">
                <div className="form-group">
                  <label className="form-label"><Phone size={14} /> Teléfono</label>
                  <input className="form-input" placeholder="+593 99 999 9999" value={form.telefono}
                    onChange={(e) => updateField('telefono', e.target.value)} />
                </div>
                <div className="form-group">
                  <label className="form-label"><MapPin size={14} /> Dirección</label>
                  <input className="form-input" placeholder="Av. Principal 123" value={form.direccion}
                    onChange={(e) => updateField('direccion', e.target.value)} />
                </div>
              </div>
              <hr className="register-divider" />
            </>
          )}

          {mode === 'existente' && (
            <div className="form-group">
              <label className="form-label">Cédula o ID de Cliente *</label>
              <input className="form-input" placeholder="1712345678 o CLT-001"
                value={form.idClienteExistente}
                onChange={(e) => updateField('idClienteExistente', e.target.value)} required />
            </div>
          )}

          {/* User account fields */}
          <div className="form-group">
            <label className="form-label"><User size={14} /> Nombre de Usuario *</label>
            <input className="form-input" placeholder="juan.perez" value={form.username}
              onChange={(e) => updateField('username', e.target.value)} required />
          </div>
          <div className="form-group">
            <label className="form-label"><Mail size={14} /> Correo Electrónico *</label>
            <input className="form-input" type="email" placeholder="juan@ejemplo.com" value={form.correo}
              onChange={(e) => updateField('correo', e.target.value)} required />
          </div>
          <div className="form-group">
            <label className="form-label">Contraseña *</label>
            <div className="form-input-wrapper">
              <input type={showPassword ? 'text' : 'password'} className="form-input"
                placeholder="Mínimo 6 caracteres" value={form.password}
                onChange={(e) => updateField('password', e.target.value)} required />
              <button type="button" className="form-input-toggle" tabIndex={-1}
                onClick={() => setShowPassword(!showPassword)}>
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Confirmar Contraseña *</label>
            <input type="password" className="form-input" placeholder="Repite tu contraseña"
              value={form.confirmPassword}
              onChange={(e) => updateField('confirmPassword', e.target.value)} required />
          </div>

          <button type="submit" className="btn btn--primary btn--full" disabled={loading}>
            {loading ? (
              <><Loader2 size={18} className="spin" /> Creando cuenta...</>
            ) : (
              <><UserPlus size={18} /> Crear Cuenta</>
            )}
          </button>
        </form>

        <div className="login-card__footer register-footer">
          <p>¿Ya tienes cuenta? <Link to="/login" className="register-link">Inicia sesión</Link></p>
          <Link to="/" className="login-card__back">← Volver al inicio</Link>
        </div>
      </div>
    </div>
  );
}
