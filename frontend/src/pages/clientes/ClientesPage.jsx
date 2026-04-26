import { useState, useEffect } from 'react';
import { clientesApi } from '../../api/clientesApi';
import { toast } from 'sonner';
import { Plus, Pencil, Trash2, Search, X, Loader2, Users } from 'lucide-react';

const INITIAL_FORM = {
  tipoIdentificacion: 'CED', numeroIdentificacion: '', nombre1: '', nombre2: '',
  apellido1: '', apellido2: '', fechaNacimiento: '', telefono: '', correo: '', direccionPrincipal: '',
};

export default function ClientesPage() {
  const [clientes, setClientes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(INITIAL_FORM);
  const [saving, setSaving] = useState(false);

  useEffect(() => { loadClientes(); }, []);

  const loadClientes = async () => {
    setLoading(true);
    try {
      const res = await clientesApi.getAll();
      setClientes(res.data?.data || []);
    } catch (e) {
      toast.error('Error al cargar clientes');
    } finally { setLoading(false); }
  };

  const openCreate = () => {
    setForm(INITIAL_FORM);
    setEditingId(null);
    setShowModal(true);
  };

  const openEdit = (cliente) => {
    setForm({
      tipoIdentificacion: cliente.tipoIdentificacion || 'CED',
      numeroIdentificacion: cliente.numeroIdentificacion || '',
      nombre1: cliente.nombre1 || '', nombre2: cliente.nombre2 || '',
      apellido1: cliente.apellido1 || '', apellido2: cliente.apellido2 || '',
      fechaNacimiento: cliente.fechaNacimiento || '',
      telefono: cliente.telefono || '', correo: cliente.correo || '',
      direccionPrincipal: cliente.direccionPrincipal || '',
      rowVersion: cliente.rowVersion,
    });
    setEditingId(cliente.idCliente);
    setShowModal(true);
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (editingId) {
        await clientesApi.update(editingId, form);
        toast.success('Cliente actualizado');
      } else {
        await clientesApi.create(form);
        toast.success('Cliente creado');
      }
      setShowModal(false);
      loadClientes();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al guardar');
    } finally { setSaving(false); }
  };

  const handleDelete = async (id) => {
    if (!confirm('¿Eliminar este cliente?')) return;
    try {
      await clientesApi.delete(id);
      toast.success('Cliente eliminado');
      loadClientes();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al eliminar');
    }
  };

  const filtered = clientes.filter((c) => {
    const text = `${c.nombreCompleto} ${c.numeroIdentificacion} ${c.correo}`.toLowerCase();
    return text.includes(search.toLowerCase());
  });

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div>
          <h1><Users size={24} /> Clientes</h1>
          <p>{clientes.length} clientes registrados</p>
        </div>
        <button className="btn btn--primary" onClick={openCreate}>
          <Plus size={16} /> Nuevo Cliente
        </button>
      </div>

      <div className="module-page__toolbar">
        <div className="search-box">
          <Search size={16} />
          <input placeholder="Buscar por nombre, identificación o correo..." value={search}
            onChange={(e) => setSearch(e.target.value)} />
        </div>
      </div>

      {loading ? (
        <div className="module-loading"><Loader2 size={24} className="spin" /> Cargando...</div>
      ) : (
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th><th>Identificación</th><th>Nombre Completo</th>
                <th>Teléfono</th><th>Correo</th><th>Estado</th><th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((c) => (
                <tr key={c.idCliente}>
                  <td>{c.idCliente}</td>
                  <td><span className="badge badge--outline">{c.tipoIdentificacion}</span> {c.numeroIdentificacion}</td>
                  <td>{c.nombreCompleto}</td>
                  <td>{c.telefono}</td>
                  <td>{c.correo}</td>
                  <td><span className={`status-badge status-badge--${c.estadoCliente === 'ACT' ? 'success' : 'danger'}`}>{c.estadoCliente}</span></td>
                  <td className="table-actions">
                    <button className="icon-btn" onClick={() => openEdit(c)} title="Editar"><Pencil size={15} /></button>
                    <button className="icon-btn icon-btn--danger" onClick={() => handleDelete(c.idCliente)} title="Eliminar"><Trash2 size={15} /></button>
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && <tr><td colSpan={7} className="table-empty">No se encontraron clientes</td></tr>}
            </tbody>
          </table>
        </div>
      )}

      {/* Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal__header">
              <h2>{editingId ? 'Editar Cliente' : 'Nuevo Cliente'}</h2>
              <button className="icon-btn" onClick={() => setShowModal(false)}><X size={18} /></button>
            </div>
            <form onSubmit={handleSave} className="modal__body">
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Tipo ID</label>
                  <select className="form-input" value={form.tipoIdentificacion}
                    onChange={(e) => setForm({...form, tipoIdentificacion: e.target.value})}>
                    <option value="CED">Cédula</option><option value="PAS">Pasaporte</option>
                    <option value="DNI">DNI</option><option value="RUC">RUC</option>
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Número ID</label>
                  <input className="form-input" required value={form.numeroIdentificacion}
                    onChange={(e) => setForm({...form, numeroIdentificacion: e.target.value})} />
                </div>
              </div>
              <div className="form-row">
                <div className="form-group"><label className="form-label">Primer Nombre</label>
                  <input className="form-input" required value={form.nombre1} onChange={(e) => setForm({...form, nombre1: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Segundo Nombre</label>
                  <input className="form-input" value={form.nombre2 || ''} onChange={(e) => setForm({...form, nombre2: e.target.value})} /></div>
              </div>
              <div className="form-row">
                <div className="form-group"><label className="form-label">Primer Apellido</label>
                  <input className="form-input" required value={form.apellido1} onChange={(e) => setForm({...form, apellido1: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Segundo Apellido</label>
                  <input className="form-input" value={form.apellido2 || ''} onChange={(e) => setForm({...form, apellido2: e.target.value})} /></div>
              </div>
              <div className="form-row">
                <div className="form-group"><label className="form-label">Fecha Nacimiento</label>
                  <input type="date" className="form-input" required value={form.fechaNacimiento} onChange={(e) => setForm({...form, fechaNacimiento: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Teléfono</label>
                  <input className="form-input" required value={form.telefono} onChange={(e) => setForm({...form, telefono: e.target.value})} /></div>
              </div>
              <div className="form-group"><label className="form-label">Correo</label>
                <input type="email" className="form-input" required value={form.correo} onChange={(e) => setForm({...form, correo: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Dirección</label>
                <input className="form-input" value={form.direccionPrincipal || ''} onChange={(e) => setForm({...form, direccionPrincipal: e.target.value})} /></div>
              <div className="modal__footer">
                <button type="button" className="btn btn--ghost" onClick={() => setShowModal(false)}>Cancelar</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>
                  {saving ? <><Loader2 size={16} className="spin" /> Guardando...</> : 'Guardar'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
