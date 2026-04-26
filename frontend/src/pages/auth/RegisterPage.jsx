import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authApi } from '../../api/authApi';
import { Car, UserPlus, Eye, EyeOff, Loader2, User, Mail, Phone, MapPin, CheckCircle2, AlertCircle } from 'lucide-react';
import { toast } from 'sonner';
import { validators } from '../../utils/validation';

export default function RegisterPage() {
  const navigate = useNavigate();
  const [mode, setMode] = useState('nuevo');
  const [step, setStep] = useState(1);
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [touched, setTouched] = useState({});
  const [shake, setShake] = useState(false);

  const [form, setForm] = useState({
    username: '', correo: '', password: '', confirmPassword: '',
    nombre: '', apellido: '', telefono: '', direccion: '', cedula: '',
    idClienteExistente: '',
  });

  const updateField = (field, value) => setForm(prev => ({ ...prev, [field]: value }));
  const handleBlur = (field) => setTouched(p => ({ ...p, [field]: true }));

  // Compute errors
  const errors = {};
  errors.username = validators.required(form.username, 'El usuario') || validators.username(form.username);
  errors.correo = validators.required(form.correo, 'El correo') || validators.email(form.correo);
  errors.password = validators.required(form.password, 'La contraseña') || validators.minLength(form.password, 6, 'La contraseña');
  errors.confirmPassword = validators.required(form.confirmPassword, 'La confirmación') || validators.match(form.confirmPassword, form.password, 'Las contraseñas');
  if (mode === 'nuevo') {
    errors.nombre = validators.required(form.nombre, 'El nombre');
    errors.apellido = validators.required(form.apellido, 'El apellido');
    errors.cedula = validators.required(form.cedula, 'La cédula') || validators.cedula(form.cedula);
    errors.telefono = form.telefono ? validators.phone(form.telefono) : '';
  }
  if (mode === 'existente') {
    errors.idClienteExistente = validators.required(form.idClienteExistente, 'El ID de cliente');
  }

  const formValid = Object.values(errors).every(e => !e);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    // Touch all fields to show errors
    const allTouched = {};
    Object.keys(form).forEach(k => allTouched[k] = true);
    setTouched(allTouched);

    if (!formValid) {
      setShake(true);
      setTimeout(() => setShake(false), 500);
      const firstErr = Object.values(errors).find(e => e);
      if (firstErr) toast.error(firstErr);
      return;
    }

    setLoading(true);
    try {
      const payload = { username: form.username, correo: form.correo, password: form.password };
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

  const renderField = (field, children) => (
    <div className={`form-group ${touched[field] && errors[field] ? 'form-group--error' : ''}`}>
      {children}
      {touched[field] && errors[field] && <span className="form-error"><AlertCircle size={13} /> {errors[field]}</span>}
    </div>
  );

  if (step === 2) {
    return (
      <div className="login-page">
        <div className="login-bg"><div className="login-bg__gradient" /></div>
        <div className="login-card register-card">
          <div className="register-success">
            <CheckCircle2 size={56} className="register-success__icon" />
            <h1>¡Cuenta Creada!</h1>
            <p>Tu cuenta ha sido registrada exitosamente.</p>
            <p className="register-success__user">Usuario: <strong>{form.username}</strong></p>
            <div className="register-success__actions">
              <Link to="/login" className="btn btn--primary btn--full">Iniciar Sesión</Link>
              <Link to="/catalogo" className="btn btn--outline btn--full">Explorar Catálogo</Link>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="login-page">
      <div className="login-bg"><div className="login-bg__gradient" /></div>
      <div className={`login-card register-card ${shake ? 'form-shake' : ''}`}>
        <div className="login-card__header">
          <div className="login-card__logo"><UserPlus size={36} /></div>
          <h1 className="login-card__title">Crear Cuenta</h1>
          <p className="login-card__subtitle">Regístrate para reservar vehículos</p>
        </div>

        <div className="login-tabs">
          <button type="button" className={`login-tab ${mode === 'nuevo' ? 'login-tab--active' : ''}`}
            onClick={() => { setMode('nuevo'); setError(''); }}>
            <UserPlus size={16} /><span>Nuevo Cliente</span>
          </button>
          <button type="button" className={`login-tab ${mode === 'existente' ? 'login-tab--active' : ''}`}
            onClick={() => { setMode('existente'); setError(''); }}>
            <User size={16} /><span>Cliente Existente</span>
          </button>
        </div>

        <form className="login-form register-form" onSubmit={handleSubmit} noValidate>
          {error && <div className="login-form__error">{error}</div>}

          {mode === 'existente' && (
            <div className="register-info"><p>Si ya eres cliente, ingresa tu cédula o ID para vincular tu cuenta.</p></div>
          )}

          {mode === 'nuevo' && (
            <>
              <div className="register-row">
                {renderField('nombre', <>
                  <label className="form-label"><User size={14} /> Nombre *</label>
                  <input className="form-input" placeholder="Juan" value={form.nombre}
                    onChange={(e) => updateField('nombre', e.target.value)} onBlur={() => handleBlur('nombre')} />
                </>)}
                {renderField('apellido', <>
                  <label className="form-label">Apellido *</label>
                  <input className="form-input" placeholder="Pérez" value={form.apellido}
                    onChange={(e) => updateField('apellido', e.target.value)} onBlur={() => handleBlur('apellido')} />
                </>)}
              </div>
              {renderField('cedula', <>
                <label className="form-label">Cédula / Pasaporte *</label>
                <input className="form-input" placeholder="1712345678" value={form.cedula}
                  onChange={(e) => updateField('cedula', e.target.value)} onBlur={() => handleBlur('cedula')} />
              </>)}
              <div className="register-row">
                {renderField('telefono', <>
                  <label className="form-label"><Phone size={14} /> Teléfono</label>
                  <input className="form-input" placeholder="+593 99 999 9999" value={form.telefono}
                    onChange={(e) => updateField('telefono', e.target.value)} onBlur={() => handleBlur('telefono')} />
                </>)}
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
            renderField('idClienteExistente', <>
              <label className="form-label">Cédula o ID de Cliente *</label>
              <input className="form-input" placeholder="1712345678 o CLT-001" value={form.idClienteExistente}
                onChange={(e) => updateField('idClienteExistente', e.target.value)} onBlur={() => handleBlur('idClienteExistente')} />
            </>)
          )}

          {renderField('username', <>
            <label className="form-label"><User size={14} /> Nombre de Usuario *</label>
            <input className="form-input" placeholder="juan.perez" value={form.username}
              onChange={(e) => updateField('username', e.target.value)} onBlur={() => handleBlur('username')} />
          </>)}
          {renderField('correo', <>
            <label className="form-label"><Mail size={14} /> Correo Electrónico *</label>
            <input className="form-input" type="email" placeholder="juan@ejemplo.com" value={form.correo}
              onChange={(e) => updateField('correo', e.target.value)} onBlur={() => handleBlur('correo')} />
          </>)}
          {renderField('password', <>
            <label className="form-label">Contraseña *</label>
            <div className="form-input-wrapper">
              <input type={showPassword ? 'text' : 'password'} className="form-input"
                placeholder="Mínimo 6 caracteres" value={form.password}
                onChange={(e) => updateField('password', e.target.value)} onBlur={() => handleBlur('password')} />
              <button type="button" className="form-input-toggle" tabIndex={-1}
                onClick={() => setShowPassword(!showPassword)}>
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
          </>)}
          {renderField('confirmPassword', <>
            <label className="form-label">Confirmar Contraseña *</label>
            <input type="password" className="form-input" placeholder="Repite tu contraseña"
              value={form.confirmPassword}
              onChange={(e) => updateField('confirmPassword', e.target.value)} onBlur={() => handleBlur('confirmPassword')} />
          </>)}

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
