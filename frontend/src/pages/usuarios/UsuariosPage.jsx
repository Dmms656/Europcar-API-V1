import { useState } from 'react';
import { Users, Plus, Edit3, Trash2, Shield, Search, X, Save, Eye, EyeOff, Key } from 'lucide-react';
import { toast } from 'sonner';

const ROLES_DISPONIBLES = ['ADMIN', 'AGENTE_POS', 'CLIENTE'];

const usuariosMock = [
  { id: 1, username: 'admin.dev', correo: 'admin@europcar.ec', roles: ['ADMIN'], estado: 'ACTIVO', fechaCreacion: '2026-01-15' },
  { id: 2, username: 'agente.quito', correo: 'agente1@europcar.ec', roles: ['AGENTE_POS'], estado: 'ACTIVO', fechaCreacion: '2026-02-20' },
  { id: 3, username: 'cliente.web', correo: 'cliente@mail.com', roles: ['CLIENTE'], estado: 'ACTIVO', fechaCreacion: '2026-03-10' },
  { id: 4, username: 'agente.gye', correo: 'agente2@europcar.ec', roles: ['AGENTE_POS'], estado: 'INACTIVO', fechaCreacion: '2026-01-05' },
];

export default function UsuariosPage() {
  const [usuarios, setUsuarios] = useState(usuariosMock);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [showPassword, setShowPassword] = useState(false);

  const [form, setForm] = useState({
    username: '', correo: '', password: '', roles: [], estado: 'ACTIVO',
  });

  const filtered = usuarios.filter(u =>
    u.username.toLowerCase().includes(search.toLowerCase()) ||
    u.correo.toLowerCase().includes(search.toLowerCase()) ||
    u.roles.some(r => r.toLowerCase().includes(search.toLowerCase()))
  );

  const openCreate = () => {
    setEditingUser(null);
    setForm({ username: '', correo: '', password: '', roles: [], estado: 'ACTIVO' });
    setShowModal(true);
  };

  const openEdit = (user) => {
    setEditingUser(user);
    setForm({ username: user.username, correo: user.correo, password: '', roles: [...user.roles], estado: user.estado });
    setShowModal(true);
  };

  const toggleRole = (role) => {
    setForm(prev => ({
      ...prev,
      roles: prev.roles.includes(role) ? prev.roles.filter(r => r !== role) : [...prev.roles, role]
    }));
  };

  const handleSave = () => {
    if (!form.username || !form.correo || form.roles.length === 0) {
      toast.error('Completa todos los campos requeridos');
      return;
    }
    if (!editingUser && !form.password) {
      toast.error('La contraseña es requerida para nuevos usuarios');
      return;
    }

    if (editingUser) {
      setUsuarios(prev => prev.map(u => u.id === editingUser.id
        ? { ...u, username: form.username, correo: form.correo, roles: form.roles, estado: form.estado }
        : u
      ));
      toast.success('Usuario actualizado');
    } else {
      setUsuarios(prev => [...prev, {
        id: Date.now(), username: form.username, correo: form.correo,
        roles: form.roles, estado: form.estado, fechaCreacion: new Date().toISOString().slice(0, 10),
      }]);
      toast.success('Usuario creado');
    }
    setShowModal(false);
  };

  const handleDelete = (user) => {
    if (user.username === 'admin.dev') {
      toast.error('No se puede eliminar al administrador principal');
      return;
    }
    setUsuarios(prev => prev.filter(u => u.id !== user.id));
    toast.success(`Usuario ${user.username} eliminado`);
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
          <p className="page-subtitle">Administra cuentas de usuario y permisos</p>
        </div>
        <button className="btn btn--primary" onClick={openCreate}>
          <Plus size={18} /> Nuevo Usuario
        </button>
      </div>

      <div className="module-page__toolbar">
        <div className="search-box">
          <Search size={18} />
          <input type="text" placeholder="Buscar por usuario, correo o rol..." value={search}
            onChange={(e) => setSearch(e.target.value)} />
        </div>
        <span className="module-page__count">{filtered.length} usuarios</span>
      </div>

      <div className="data-table-wrapper">
        <table className="data-table">
          <thead>
            <tr>
              <th>Usuario</th>
              <th>Correo</th>
              <th>Roles</th>
              <th>Estado</th>
              <th>Creado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((u) => (
              <tr key={u.id}>
                <td>
                  <div className="user-cell">
                    <div className="user-cell__avatar">{u.username.charAt(0).toUpperCase()}</div>
                    <span className="user-cell__name">{u.username}</span>
                  </div>
                </td>
                <td>{u.correo}</td>
                <td>
                  <div className="role-tags">
                    {u.roles.map(r => (
                      <span key={r} className="role-tag" style={{ borderColor: roleColor(r), color: roleColor(r) }}>
                        <Shield size={12} /> {r}
                      </span>
                    ))}
                  </div>
                </td>
                <td>
                  <span className={`status-badge status-badge--${u.estado.toLowerCase()}`}>
                    {u.estado}
                  </span>
                </td>
                <td>{new Date(u.fechaCreacion).toLocaleDateString('es-EC')}</td>
                <td>
                  <div className="table-actions">
                    <button className="btn btn--ghost btn--sm" onClick={() => openEdit(u)}>
                      <Edit3 size={15} />
                    </button>
                    <button className="btn btn--ghost btn--sm btn--danger" onClick={() => handleDelete(u)}>
                      <Trash2 size={15} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal__header">
              <h2>{editingUser ? 'Editar Usuario' : 'Nuevo Usuario'}</h2>
              <button className="btn btn--ghost btn--sm" onClick={() => setShowModal(false)}><X size={18} /></button>
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
                  <Key size={14} /> {editingUser ? 'Nueva Contraseña (dejar vacío para mantener)' : 'Contraseña *'}
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
                <label className="form-label">Estado</label>
                <select className="form-input" value={form.estado}
                  onChange={(e) => setForm({ ...form, estado: e.target.value })}>
                  <option value="ACTIVO">Activo</option>
                  <option value="INACTIVO">Inactivo</option>
                </select>
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
              <button className="btn btn--primary" onClick={handleSave}>
                <Save size={16} /> {editingUser ? 'Guardar Cambios' : 'Crear Usuario'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
