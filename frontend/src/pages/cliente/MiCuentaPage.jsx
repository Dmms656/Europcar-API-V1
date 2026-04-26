import { useState } from 'react';
import { useAuthStore } from '../../store/useAuthStore';
import { User, Mail, Phone, MapPin, Edit3, Save, X, ShieldCheck } from 'lucide-react';
import { toast } from 'sonner';

export default function MiCuentaPage() {
  const { user } = useAuthStore();
  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState({
    nombre: user?.nombreCompleto || user?.username || '',
    correo: user?.correo || '',
    telefono: '',
    direccion: '',
  });

  const handleSave = () => {
    toast.success('Datos actualizados correctamente');
    setEditing(false);
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
                <button className="btn btn--primary btn--sm" onClick={handleSave}>
                  <Save size={16} /> Guardar
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
            <div className="cuenta-field">
              <label><Mail size={16} /> Correo Electrónico</label>
              {editing ? (
                <input className="form-input" type="email" value={form.correo}
                  onChange={(e) => setForm({ ...form, correo: e.target.value })} />
              ) : (
                <p>{form.correo || '—'}</p>
              )}
            </div>
            <div className="cuenta-field">
              <label><Phone size={16} /> Teléfono</label>
              {editing ? (
                <input className="form-input" value={form.telefono}
                  onChange={(e) => setForm({ ...form, telefono: e.target.value })} />
              ) : (
                <p>{form.telefono || '—'}</p>
              )}
            </div>
            <div className="cuenta-field">
              <label><MapPin size={16} /> Dirección</label>
              {editing ? (
                <input className="form-input" value={form.direccion}
                  onChange={(e) => setForm({ ...form, direccion: e.target.value })} />
              ) : (
                <p>{form.direccion || '—'}</p>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
