import { useState, useEffect } from 'react';
import { mantenimientosApi } from '../../api/mantenimientosApi';
import { toast } from 'sonner';
import { Wrench, Search, Plus, X, Loader2, CheckCircle, RefreshCw } from 'lucide-react';

export default function MantenimientosPage() {
  const [mantenimientos, setMantenimientos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({
    idVehiculo: '', tipoMantenimiento: 'PREVENTIVO', descripcion: '', costoEstimado: '',
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

  const handleCreate = async (e) => {
    e.preventDefault(); setSaving(true);
    try {
      await mantenimientosApi.create({ ...form, idVehiculo: Number(form.idVehiculo), costoEstimado: Number(form.costoEstimado) });
      toast.success('Mantenimiento registrado');
      setShowModal(false);
      loadMantenimientos();
    } catch (e) { toast.error(e.response?.data?.message || 'Error'); }
    finally { setSaving(false); }
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

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div><h1><Wrench size={24} /> Mantenimientos</h1><p>{mantenimientos.length} mantenimientos registrados</p></div>
        <div className="module-page__actions">
          <button className="btn btn--outline btn--sm" onClick={loadMantenimientos} disabled={loading}>
            <RefreshCw size={16} className={loading ? 'spin' : ''} /> Recargar
          </button>
          <button className="btn btn--primary" onClick={() => setShowModal(true)}><Plus size={16} /> Nuevo Mantenimiento</button>
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
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead><tr><th>Código</th><th>Vehículo</th><th>Tipo</th><th>Descripción</th><th>Costo</th><th>Inicio</th><th>Fin</th><th>Estado</th><th>Acciones</th></tr></thead>
            <tbody>
              {filtered.map(m => (
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
      )}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal__header"><h2>Nuevo Mantenimiento</h2><button className="icon-btn" onClick={() => setShowModal(false)}><X size={18} /></button></div>
            <form onSubmit={handleCreate} className="modal__body">
              <div className="form-row">
                <div className="form-group"><label className="form-label">ID Vehículo</label>
                  <input type="number" className="form-input" required value={form.idVehiculo} onChange={e => setForm({...form, idVehiculo: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Tipo</label>
                  <select className="form-input" value={form.tipoMantenimiento} onChange={e => setForm({...form, tipoMantenimiento: e.target.value})}>
                    <option value="PREVENTIVO">Preventivo</option><option value="CORRECTIVO">Correctivo</option><option value="PREDICTIVO">Predictivo</option>
                  </select></div>
              </div>
              <div className="form-group"><label className="form-label">Descripción</label>
                <input className="form-input" required value={form.descripcion} onChange={e => setForm({...form, descripcion: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Costo Estimado ($)</label>
                <input type="number" step="0.01" className="form-input" value={form.costoEstimado} onChange={e => setForm({...form, costoEstimado: e.target.value})} /></div>
              <div className="modal__footer">
                <button type="button" className="btn btn--ghost" onClick={() => setShowModal(false)}>Cancelar</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>{saving ? 'Registrando...' : 'Crear'}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
