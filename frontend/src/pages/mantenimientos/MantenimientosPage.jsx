import { useState, useEffect } from 'react';
import { mantenimientosApi } from '../../api/mantenimientosApi';
import { toast } from 'sonner';
import { Wrench, Search, Plus, X, Loader2, CheckCircle, RefreshCw, Pencil } from 'lucide-react';
import { useClientPagination } from '../../hooks/useClientPagination';
import PaginationControls from '../../components/ui/PaginationControls';

export default function MantenimientosPage() {
  const [mantenimientos, setMantenimientos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState({
    idVehiculo: '',
    tipoMantenimiento: 'PREVENTIVO',
    kilometrajeMantenimiento: '',
    costoMantenimiento: '',
    proveedorTaller: '',
    observaciones: '',
    estadoMantenimiento: 'ABIERTO',
  });

  useEffect(() => { loadMantenimientos(); }, []);

  const loadMantenimientos = async () => {
    setLoading(true);
    try {
      const res = await mantenimientosApi.getAll();
      setMantenimientos(res.data?.data || []);
    } catch (e) { toast.error('Error al cargar mantenimientos'); }
    finally { setLoading(false); }
  };

  const handleSave = async (e) => {
    e.preventDefault(); setSaving(true);
    try {
      const payload = {
        idVehiculo: Number(form.idVehiculo),
        tipoMantenimiento: form.tipoMantenimiento,
        kilometrajeMantenimiento: Number(form.kilometrajeMantenimiento || 0),
        costoMantenimiento: Number(form.costoMantenimiento || 0),
        proveedorTaller: form.proveedorTaller || null,
        observaciones: form.observaciones || null,
        estadoMantenimiento: form.estadoMantenimiento,
      };

      if (editing) {
        await mantenimientosApi.update(editing.idMantenimiento, payload);
        toast.success('Mantenimiento actualizado');
      } else {
        await mantenimientosApi.create(payload);
        toast.success('Mantenimiento registrado');
      }

      setShowModal(false);
      setEditing(null);
      loadMantenimientos();
    } catch (e) { toast.error(e.response?.data?.message || 'Error'); }
    finally { setSaving(false); }
  };

  const openCreate = () => {
    setEditing(null);
    setForm({
      idVehiculo: '',
      tipoMantenimiento: 'PREVENTIVO',
      kilometrajeMantenimiento: '',
      costoMantenimiento: '',
      proveedorTaller: '',
      observaciones: '',
      estadoMantenimiento: 'ABIERTO',
    });
    setShowModal(true);
  };

  const openEdit = (m) => {
    setEditing(m);
    setForm({
      idVehiculo: String(m.idVehiculo || ''),
      tipoMantenimiento: m.tipoMantenimiento || 'PREVENTIVO',
      kilometrajeMantenimiento: String(m.kilometrajeMantenimiento || 0),
      costoMantenimiento: String(m.costoMantenimiento || 0),
      proveedorTaller: m.proveedorTaller || '',
      observaciones: m.observaciones || '',
      estadoMantenimiento: m.estadoMantenimiento || 'ABIERTO',
    });
    setShowModal(true);
  };

  const cerrar = async (id) => {
    if (!confirm('¿Cerrar este mantenimiento?')) return;
    try {
      await mantenimientosApi.cerrar(id, {});
      toast.success('Mantenimiento cerrado');
      loadMantenimientos();
    } catch (e) { toast.error(e.response?.data?.message || 'Error'); }
  };

  const filtered = mantenimientos.filter(m => {
    const text = `${m.codigoMantenimiento || ''} ${m.placaVehiculo || ''} ${m.tipoMantenimiento || ''} ${m.estadoMantenimiento || ''}`.toLowerCase();
    return text.includes(search.toLowerCase());
  });
  const pagination = useClientPagination(filtered, 10);

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div><h1><Wrench size={24} /> Mantenimientos</h1><p>{mantenimientos.length} mantenimientos registrados</p></div>
        <div className="module-page__actions">
          <button className="btn btn--outline btn--sm" onClick={loadMantenimientos} disabled={loading}>
            <RefreshCw size={16} className={loading ? 'spin' : ''} /> Recargar
          </button>
          <button className="btn btn--primary" onClick={openCreate}><Plus size={16} /> Nuevo Mantenimiento</button>
        </div>
      </div>
      <div className="module-page__toolbar">
        <div className="search-box"><Search size={16} />
          <input placeholder="Buscar por código, placa, tipo o estado..." value={search}
            onChange={(e) => setSearch(e.target.value)} />
        </div>
      </div>
      {loading ? (
        <div className="module-loading"><Loader2 size={24} className="spin" /> Cargando mantenimientos...</div>
      ) : (
        <>
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead><tr><th>Código</th><th>Vehículo</th><th>Tipo</th><th>Descripción</th><th>Costo</th><th>Inicio</th><th>Fin</th><th>Estado</th><th>Acciones</th></tr></thead>
            <tbody>
              {pagination.paginatedItems.map(m => (
                <tr key={m.idMantenimiento}>
                  <td><code>{m.codigoMantenimiento}</code></td>
                  <td><strong>{m.placaVehiculo || `#${m.idVehiculo || '-'}`}</strong></td>
                  <td><span className="badge badge--outline">{m.tipoMantenimiento}</span></td>
                  <td>{m.observaciones || '-'}</td>
                  <td>${Number(m.costoMantenimiento || 0).toFixed(2)}</td>
                  <td>{m.fechaInicioUtc ? new Date(m.fechaInicioUtc).toLocaleDateString() : '-'}</td>
                  <td>{m.fechaFinUtc ? new Date(m.fechaFinUtc).toLocaleDateString() : '-'}</td>
                  <td><span className={`status-badge status-badge--${m.estadoMantenimiento === 'CERRADO' ? 'success' : 'warning'}`}>{m.estadoMantenimiento}</span></td>
                  <td className="table-actions">
                    {m.estadoMantenimiento !== 'CERRADO' && (
                      <button className="icon-btn" onClick={() => openEdit(m)} title="Editar">
                        <Pencil size={15} />
                      </button>
                    )}
                    {m.estadoMantenimiento !== 'CERRADO' && (
                      <button className="icon-btn icon-btn--success" onClick={() => cerrar(m.idMantenimiento)} title="Cerrar">
                        <CheckCircle size={15} />
                      </button>
                    )}
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && <tr><td colSpan={9} className="table-empty">No hay mantenimientos</td></tr>}
            </tbody>
          </table>
        </div>
        <PaginationControls
          page={pagination.page}
          totalPages={pagination.totalPages}
          pageSize={pagination.pageSize}
          onPageChange={pagination.setPage}
          onPageSizeChange={pagination.setPageSize}
          totalItems={pagination.totalItems}
          startItem={pagination.startItem}
          endItem={pagination.endItem}
        />
        </>
      )}
      {showModal && (
        <div className="modal-overlay" onClick={() => { setShowModal(false); setEditing(null); }}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal__header"><h2>{editing ? 'Editar Mantenimiento' : 'Nuevo Mantenimiento'}</h2><button className="icon-btn" onClick={() => { setShowModal(false); setEditing(null); }}><X size={18} /></button></div>
            <form onSubmit={handleSave} className="modal__body">
              <div className="form-row">
                <div className="form-group"><label className="form-label">ID Vehículo</label>
                  <input type="number" className="form-input" required disabled={!!editing} value={form.idVehiculo} onChange={e => setForm({...form, idVehiculo: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Tipo</label>
                  <select className="form-input" value={form.tipoMantenimiento} onChange={e => setForm({...form, tipoMantenimiento: e.target.value})}>
                    <option value="PREVENTIVO">Preventivo</option><option value="CORRECTIVO">Correctivo</option><option value="PREDICTIVO">Predictivo</option>
                  </select></div>
              </div>
              {editing && (
                <div className="form-group">
                  <label className="form-label">Vehículo seleccionado</label>
                  <input
                    className="form-input"
                    disabled
                    value={`#${form.idVehiculo} - ${editing.placaVehiculo || 'Placa no disponible'}`}
                  />
                </div>
              )}
              <div className="form-row">
                <div className="form-group"><label className="form-label">Kilometraje</label>
                  <input type="number" className="form-input" value={form.kilometrajeMantenimiento} onChange={e => setForm({...form, kilometrajeMantenimiento: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Costo ($)</label>
                  <input type="number" step="0.01" className="form-input" value={form.costoMantenimiento} onChange={e => setForm({...form, costoMantenimiento: e.target.value})} /></div>
              </div>
              <div className="form-group"><label className="form-label">Proveedor</label>
                <input className="form-input" value={form.proveedorTaller} onChange={e => setForm({...form, proveedorTaller: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Observaciones</label>
                <input className="form-input" value={form.observaciones} onChange={e => setForm({...form, observaciones: e.target.value})} /></div>
              {editing && (
                <div className="form-group"><label className="form-label">Estado</label>
                  <select className="form-input" value={form.estadoMantenimiento} onChange={e => setForm({...form, estadoMantenimiento: e.target.value})}>
                    <option value="ABIERTO">Abierto</option>
                    <option value="CERRADO">Cerrado</option>
                  </select></div>
              )}
              <div className="modal__footer">
                <button type="button" className="btn btn--ghost" onClick={() => { setShowModal(false); setEditing(null); }}>Cancelar</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>{saving ? 'Guardando...' : (editing ? 'Guardar cambios' : 'Crear')}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
