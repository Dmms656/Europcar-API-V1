import { useState, useEffect } from 'react';
import { usuariosApi } from '../../api/usuariosApi';
import { Users, Plus, Trash2, Shield, Search, X, Eye, EyeOff, Key, Loader2, RefreshCw, ToggleLeft, ToggleRight } from 'lucide-react';
import { toast } from 'sonner';

const ROLES_DISPONIBLES = ['ADMIN', 'AGENTE_POS', 'CLIENTE'];

export default function UsuariosPage() {
  const [usuarios, setUsuarios] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState({
    username: '', correo: '', password: '', roles: [],
  });

  useEffect(() => { loadUsuarios(); }, []);

  const loadUsuarios = async () => {
    setLoading(true);
    try {
      const res = await usuariosApi.getAll();
      setUsuarios(res.data?.data || []);
    } catch (e) {
      toast.error('Error al cargar usuarios');
    } finally { setLoading(false); }
  };

  const filtered = usuarios.filter(u =>
    u.username?.toLowerCase().includes(search.toLowerCase()) ||
    u.correo?.toLowerCase().includes(search.toLowerCase()) ||
    u.roles?.some(r => r.toLowerCase().includes(search.toLowerCase()))
  );

  const openCreate = () => {
    setForm({ username: '', correo: '', password: '', roles: [] });
    setShowModal(true);
  };

  const toggleRole = (role) => {
    setForm(prev => ({
      ...prev,
      roles: prev.roles.includes(role) ? prev.roles.filter(r => r !== role) : [...prev.roles, role]
    }));
  };

  const handleSave = async () => {
    if (!form.username || !form.correo || !form.password || form.roles.length === 0) {
      toast.error('Completa todos los campos requeridos');
      return;
    }
    setSaving(true);
    try {
      await usuariosApi.create(form);
      toast.success('Usuario creado');
      setShowModal(false);
      loadUsuarios();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al crear usuario');
    } finally { setSaving(false); }
  };

  const toggleEstado = async (user) => {
    const newEstado = user.activo ? 'INA' : 'ACT';
    try {
      await usuariosApi.updateEstado(user.idUsuario, newEstado);
      toast.success(`Usuario ${newEstado === 'ACT' ? 'activado' : 'desactivado'}`);
      loadUsuarios();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al cambiar estado');
    }
  };

  const handleDelete = async (user) => {
    if (!confirm(`¿Eliminar al usuario "${user.username}"? Esta acción no se puede deshacer.`)) return;
    try {
      await usuariosApi.delete(user.idUsuario);
      toast.success(`Usuario ${user.username} eliminado`);
      loadUsuarios();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al eliminar');
    }
  };

  const roleColor = (role) => {
    switch (role) {
      case 'ADMIN': return 'var(--color-danger)';
      case 'AGENTE_POS': return 'var(--color-info)';
      case 'CLIENTE': return 'var(--color-success)';
      default: return 'var(--color-text-muted)';
    }
  };

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div>
          <h1><Users size={28} /> Gestión de Usuarios</h1>
          <p>{usuarios.length} usuarios en el sistema</p>
        </div>
        <div className="module-page__actions">
          <button className="btn btn--outline btn--sm" onClick={loadUsuarios} disabled={loading}>
            <RefreshCw size={16} className={loading ? 'spin' : ''} /> Recargar
          </button>
          <button className="btn btn--primary" onClick={openCreate}>
            <Plus size={18} /> Nuevo Usuario
          </button>
        </div>
      </div>

      <div className="module-page__toolbar">
        <div className="search-box">
          <Search size={18} />
          <input type="text" placeholder="Buscar por usuario, correo o rol..." value={search}
            onChange={(e) => setSearch(e.target.value)} />
        </div>
      </div>

      {loading ? (
        <div className="module-loading"><Loader2 size={24} className="spin" /> Cargando usuarios...</div>
      ) : (
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Usuario</th>
                <th>Correo</th>
                <th>Roles</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((u) => (
                <tr key={u.idUsuario}>
                  <td>{u.idUsuario}</td>
                  <td>
                    <div className="user-cell">
                      <div className="user-cell__avatar">{u.username?.charAt(0).toUpperCase()}</div>
                      <span className="user-cell__name">{u.username}</span>
                    </div>
                  </td>
                  <td>{u.correo}</td>
                  <td>
                    <div className="role-tags">
                      {u.roles?.map(r => (
                        <span key={r} className="role-tag" style={{ borderColor: roleColor(r), color: roleColor(r) }}>
                          <Shield size={12} /> {r}
                        </span>
                      ))}
                    </div>
                  </td>
                  <td>
                    <span className={`status-badge status-badge--${u.activo ? 'success' : 'danger'}`}>
                      {u.activo ? 'ACTIVO' : 'INACTIVO'}
                    </span>
                  </td>
                  <td>
                    <div className="table-actions">
                      <button className="icon-btn" onClick={() => toggleEstado(u)}
                        title={u.activo ? 'Desactivar' : 'Activar'}>
                        {u.activo ? <ToggleRight size={18} color="var(--color-success)" /> : <ToggleLeft size={18} />}
                      </button>
                      <button className="icon-btn icon-btn--danger" onClick={() => handleDelete(u)} title="Eliminar">
                        <Trash2 size={15} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && <tr><td colSpan={6} className="table-empty">No se encontraron usuarios</td></tr>}
            </tbody>
          </table>
        </div>
      )}

      {/* Modal Crear Usuario */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal__header">
              <h2>Nuevo Usuario</h2>
              <button className="icon-btn" onClick={() => setShowModal(false)}><X size={18} /></button>
            </div>
            <div className="modal__body">
              <div className="form-group">
                <label className="form-label">Nombre de Usuario *</label>
                <input className="form-input" value={form.username} placeholder="usuario.ejemplo"
                  onChange={(e) => setForm({ ...form, username: e.target.value })} />
              </div>
              <div className="form-group">
                <label className="form-label">Correo Electrónico *</label>
                <input className="form-input" type="email" value={form.correo} placeholder="correo@ejemplo.com"
                  onChange={(e) => setForm({ ...form, correo: e.target.value })} />
              </div>
              <div className="form-group">
                <label className="form-label">
                  <Key size={14} /> Contraseña *
                </label>
                <div className="form-input-wrapper">
                  <input type={showPassword ? 'text' : 'password'} className="form-input"
                    value={form.password} placeholder="••••••••"
                    onChange={(e) => setForm({ ...form, password: e.target.value })} />
                  <button type="button" className="form-input-toggle" onClick={() => setShowPassword(!showPassword)}>
                    {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                  </button>
                </div>
              </div>
              <div className="form-group">
                <label className="form-label"><Shield size={14} /> Roles / Permisos *</label>
                <div className="roles-selector">
                  {ROLES_DISPONIBLES.map(role => (
                    <button key={role} type="button"
                      className={`role-option ${form.roles.includes(role) ? 'role-option--active' : ''}`}
                      style={{ '--role-color': roleColor(role) }}
                      onClick={() => toggleRole(role)}>
                      <Shield size={14} />
                      <span>{role}</span>
                    </button>
                  ))}
                </div>
              </div>
            </div>
            <div className="modal__footer">
              <button className="btn btn--ghost" onClick={() => setShowModal(false)}>Cancelar</button>
              <button className="btn btn--primary" onClick={handleSave} disabled={saving}>
                {saving ? <><Loader2 size={16} className="spin" /> Creando...</> : 'Crear Usuario'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
