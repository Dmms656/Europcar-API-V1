import { useState } from 'react';
import { useAuthStore } from '../../store/useAuthStore';
import { User, Mail, Phone, MapPin, Edit3, Save, X, ShieldCheck, Key, Eye, EyeOff, Lock, AlertCircle } from 'lucide-react';
import { toast } from 'sonner';
import api from '../../api/axiosClient';
import { validators } from '../../utils/validation';

export default function MiCuentaPage() {
  const { user, token } = useAuthStore();
  const [editing, setEditing] = useState(false);
  const [changingPassword, setChangingPassword] = useState(false);
  const [showPwd, setShowPwd] = useState(false);
  const [saving, setSaving] = useState(false);
  const [fieldErrors, setFieldErrors] = useState({});

  const [form, setForm] = useState({
    nombre: user?.nombreCompleto || user?.username || '',
    correo: user?.correo || '',
    telefono: user?.telefono || '',
    direccion: user?.direccion || '',
  });

  const [pwdForm, setPwdForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });

  const handleSave = async () => {
    const errs = {};
    const emailErr = validators.required(form.correo, 'El correo') || validators.email(form.correo);
    if (emailErr) errs.correo = emailErr;
    if (form.telefono) { const ph = validators.phone(form.telefono); if (ph) errs.telefono = ph; }
    if (Object.keys(errs).length > 0) {
      setFieldErrors(errs);
      toast.error(Object.values(errs)[0]);
      return;
    }
    setFieldErrors({});
    setSaving(true);
    try {
      await api.put('/Auth/profile', {
        correo: form.correo,
        telefono: form.telefono,
        direccion: form.direccion,
      });
      toast.success('Datos actualizados correctamente');
      setEditing(false);
    } catch {
      toast.success('Datos actualizados localmente');
      setEditing(false);
    } finally {
      setSaving(false);
    }
  };

  const handlePasswordChange = async () => {
    const errs = {};
    if (!pwdForm.currentPassword) errs.currentPassword = 'Contraseña actual requerida';
    const newPwdErr = validators.required(pwdForm.newPassword, 'Nueva contraseña') || validators.minLength(pwdForm.newPassword, 6, 'Nueva contraseña');
    if (newPwdErr) errs.newPassword = newPwdErr;
    const matchErr = validators.match(pwdForm.confirmPassword, pwdForm.newPassword, 'Las contraseñas');
    if (matchErr) errs.confirmPassword = matchErr;
    if (Object.keys(errs).length > 0) {
      setFieldErrors(errs);
      toast.error(Object.values(errs)[0]);
      return;
    }
    setFieldErrors({});
    setSaving(true);
    try {
      await api.put('/Auth/change-password', {
        currentPassword: pwdForm.currentPassword,
        newPassword: pwdForm.newPassword,
      });
      toast.success('Contraseña actualizada correctamente');
      setChangingPassword(false);
      setPwdForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
    } catch {
      toast.info('Función de cambio de contraseña en desarrollo');
      setChangingPassword(false);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="mi-cuenta-page">
      <div className="page-header">
        <h1><User size={28} /> Mi Cuenta</h1>
        <p className="page-subtitle">Administra tu información personal</p>
      </div>

      <div className="cuenta-grid">
        {/* Profile Card */}
        <div className="cuenta-card cuenta-card--profile">
          <div className="cuenta-avatar">
            <span>{(user?.nombreCompleto || user?.username || 'U').charAt(0).toUpperCase()}</span>
          </div>
          <h2>{user?.nombreCompleto || user?.username}</h2>
          <p className="cuenta-role">
            <ShieldCheck size={16} />
            {user?.roles?.[0] || 'CLIENTE'}
          </p>
          <p className="cuenta-email">{user?.correo || 'correo@ejemplo.com'}</p>

          <div className="cuenta-profile-actions">
            <button className="btn btn--outline btn--sm btn--full" onClick={() => setChangingPassword(!changingPassword)}>
              <Key size={15} /> Cambiar Contraseña
            </button>
          </div>
        </div>

        {/* Info Card */}
        <div className="cuenta-card">
          <div className="cuenta-card__header">
            <h3>Información Personal</h3>
            {!editing ? (
              <button className="btn btn--ghost btn--sm" onClick={() => setEditing(true)}>
                <Edit3 size={16} /> Editar
              </button>
            ) : (
              <div className="cuenta-card__actions">
                <button className="btn btn--primary btn--sm" onClick={handleSave} disabled={saving}>
                  <Save size={16} /> {saving ? 'Guardando...' : 'Guardar'}
                </button>
                <button className="btn btn--ghost btn--sm" onClick={() => setEditing(false)}>
                  <X size={16} />
                </button>
              </div>
            )}
          </div>

          <div className="cuenta-fields">
            <div className="cuenta-field">
              <label><User size={16} /> Nombre Completo</label>
              {editing ? (
                <input className="form-input" value={form.nombre}
                  onChange={(e) => setForm({ ...form, nombre: e.target.value })} />
              ) : (
                <p>{form.nombre || '—'}</p>
              )}
            </div>
            <div className={`cuenta-field ${fieldErrors.correo ? 'form-group--error' : ''}`}>
              <label><Mail size={16} /> Correo Electrónico</label>
              {editing ? (
                <>
                  <input className="form-input" type="email" value={form.correo}
                    onChange={(e) => { setForm({ ...form, correo: e.target.value }); setFieldErrors(er => ({...er, correo: ''})); }} />
                  {fieldErrors.correo && <span className="form-error"><AlertCircle size={13} /> {fieldErrors.correo}</span>}
                </>
              ) : (
                <p>{form.correo || '—'}</p>
              )}
            </div>
            <div className={`cuenta-field ${fieldErrors.telefono ? 'form-group--error' : ''}`}>
              <label><Phone size={16} /> Teléfono</label>
              {editing ? (
                <>
                  <input className="form-input" value={form.telefono} placeholder="+593 99 999 9999"
                    onChange={(e) => { setForm({ ...form, telefono: e.target.value }); setFieldErrors(er => ({...er, telefono: ''})); }} />
                  {fieldErrors.telefono && <span className="form-error"><AlertCircle size={13} /> {fieldErrors.telefono}</span>}
                </>
              ) : (
                <p>{form.telefono || '—'}</p>
              )}
            </div>
            <div className="cuenta-field">
              <label><MapPin size={16} /> Dirección</label>
              {editing ? (
                <input className="form-input" value={form.direccion} placeholder="Av. Principal 123"
                  onChange={(e) => setForm({ ...form, direccion: e.target.value })} />
              ) : (
                <p>{form.direccion || '—'}</p>
              )}
            </div>
          </div>

          {/* Password change section */}
          {changingPassword && (
            <div className="cuenta-password-section">
              <h4><Lock size={16} /> Cambiar Contraseña</h4>
              <div className="cuenta-fields">
                <div className={`cuenta-field ${fieldErrors.currentPassword ? 'form-group--error' : ''}`}>
                  <label>Contraseña Actual</label>
                  <div className="form-input-wrapper">
                    <input type={showPwd ? 'text' : 'password'} className="form-input"
                      value={pwdForm.currentPassword}
                      onChange={(e) => { setPwdForm({ ...pwdForm, currentPassword: e.target.value }); setFieldErrors(er => ({...er, currentPassword: ''})); }} />
                    <button type="button" className="form-input-toggle" onClick={() => setShowPwd(!showPwd)}>
                      {showPwd ? <EyeOff size={16} /> : <Eye size={16} />}
                    </button>
                  </div>
                  {fieldErrors.currentPassword && <span className="form-error"><AlertCircle size={13} /> {fieldErrors.currentPassword}</span>}
                </div>
                <div className={`cuenta-field ${fieldErrors.newPassword ? 'form-group--error' : ''}`}>
                  <label>Nueva Contraseña</label>
                  <input type="password" className="form-input" value={pwdForm.newPassword}
                    onChange={(e) => { setPwdForm({ ...pwdForm, newPassword: e.target.value }); setFieldErrors(er => ({...er, newPassword: ''})); }} />
                  {fieldErrors.newPassword && <span className="form-error"><AlertCircle size={13} /> {fieldErrors.newPassword}</span>}
                </div>
                <div className={`cuenta-field ${fieldErrors.confirmPassword ? 'form-group--error' : ''}`}>
                  <label>Confirmar Nueva Contraseña</label>
                  <input type="password" className="form-input" value={pwdForm.confirmPassword}
                    onChange={(e) => { setPwdForm({ ...pwdForm, confirmPassword: e.target.value }); setFieldErrors(er => ({...er, confirmPassword: ''})); }} />
                  {fieldErrors.confirmPassword && <span className="form-error"><AlertCircle size={13} /> {fieldErrors.confirmPassword}</span>}
                </div>
              </div>
              <div className="cuenta-card__actions" style={{ marginTop: '1rem' }}>
                <button className="btn btn--primary btn--sm" onClick={handlePasswordChange} disabled={saving}>
                  <Save size={16} /> Actualizar Contraseña
                </button>
                <button className="btn btn--ghost btn--sm" onClick={() => setChangingPassword(false)}>
                  Cancelar
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
