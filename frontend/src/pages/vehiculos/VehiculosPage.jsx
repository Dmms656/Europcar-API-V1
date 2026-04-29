import { useState, useEffect } from 'react';
import { vehiculosApi } from '../../api/vehiculosApi';
import { catalogosApi } from '../../api/catalogosApi';
import { toast } from 'sonner';
import { Plus, Pencil, Trash2, Search, X, Loader2, Car } from 'lucide-react';
import ImageUploader from '../../components/ui/ImageUploader';

const INITIAL_FORM = {
  placaVehiculo: '', idMarca: '', idCategoria: '', modeloVehiculo: '', anioFabricacion: 2024,
  colorVehiculo: '', tipoCombustible: 'GASOLINA', tipoTransmision: 'AUTOMATICA',
  capacidadPasajeros: 5, capacidadMaletas: 2, numeroPuertas: 4, idLocalizacion: '',
  precioBaseDia: '', kilometrajeActual: 0, aireAcondicionado: true,
  observacionesGenerales: '', imagenReferencialUrl: '',
};

const ESTADOS_OPERATIVOS_EDITABLES = ['DISPONIBLE', 'RESERVADO', 'MANTENIMIENTO', 'TALLER', 'FUERA_SERVICIO'];

export default function VehiculosPage() {
  const [vehiculos, setVehiculos] = useState([]);
  const [marcas, setMarcas] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [localizaciones, setLocalizaciones] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(INITIAL_FORM);
  const [saving, setSaving] = useState(false);

  useEffect(() => { loadAll(); }, []);

  const loadAll = async () => {
    setLoading(true);
    try {
      const [vRes, mRes, cRes, lRes] = await Promise.all([
        vehiculosApi.getAll(),
        catalogosApi.getMarcas(),
        catalogosApi.getCategorias(),
        catalogosApi.getLocalizaciones(),
      ]);
      setVehiculos(vRes.data?.data || []);
      setMarcas(mRes.data?.data || []);
      setCategorias(cRes.data?.data || []);
      setLocalizaciones(lRes.data?.data || []);
    } catch (e) { toast.error('Error al cargar datos'); }
    finally { setLoading(false); }
  };

  const openCreate = () => { setForm(INITIAL_FORM); setEditingId(null); setShowModal(true); };

  const openEdit = (v) => {
    setForm({
      placaVehiculo: v.placa, idMarca: v.idMarca, idCategoria: v.idCategoria,
      modeloVehiculo: v.modelo, anioFabricacion: v.anioFabricacion, colorVehiculo: v.color,
      tipoCombustible: v.tipoCombustible, tipoTransmision: v.tipoTransmision,
      capacidadPasajeros: v.capacidadPasajeros, capacidadMaletas: v.capacidadMaletas,
      numeroPuertas: v.numeroPuertas, idLocalizacion: v.idLocalizacion,
      precioBaseDia: v.precioBaseDia, kilometrajeActual: v.kilometrajeActual,
      aireAcondicionado: v.aireAcondicionado, observacionesGenerales: v.observacionesGenerales || '',
      imagenReferencialUrl: v.imagenReferencialUrl || '', rowVersion: v.rowVersion,
    });
    setEditingId(v.idVehiculo);
    setShowModal(true);
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = { ...form, idMarca: Number(form.idMarca), idCategoria: Number(form.idCategoria), idLocalizacion: Number(form.idLocalizacion), precioBaseDia: Number(form.precioBaseDia) };
      if (editingId) {
        await vehiculosApi.update(editingId, payload);
        toast.success('Vehículo actualizado');
      } else {
        await vehiculosApi.create(payload);
        toast.success('Vehículo creado');
      }
      setShowModal(false);
      loadAll();
    } catch (e) { toast.error(e.response?.data?.message || 'Error al guardar'); }
    finally { setSaving(false); }
  };

  const handleDelete = async (id) => {
    if (!confirm('¿Eliminar este vehículo?')) return;
    try { await vehiculosApi.delete(id); toast.success('Vehículo eliminado'); loadAll(); }
    catch (e) { toast.error(e.response?.data?.message || 'Error al eliminar'); }
  };

  const handleEstadoOperativoChange = async (vehiculo, nuevoEstado) => {
    if (!nuevoEstado || nuevoEstado === vehiculo.estadoOperativo) return;
    if (vehiculo.estadoOperativo === 'ALQUILADO') {
      toast.error('ALQUILADO no es editable manualmente');
      return;
    }
    try {
      await vehiculosApi.cambiarEstadoOperativo(vehiculo.idVehiculo, nuevoEstado);
      toast.success('Estado operativo actualizado');
      loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'No se pudo actualizar el estado operativo');
    }
  };

  const filtered = vehiculos.filter((v) => {
    const text = `${v.placa} ${v.marca} ${v.modelo} ${v.codigoInterno}`.toLowerCase();
    return text.includes(search.toLowerCase());
  });

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div><h1><Car size={24} /> Vehículos</h1><p>{vehiculos.length} vehículos en la flota</p></div>
        <button className="btn btn--primary" onClick={openCreate}><Plus size={16} /> Nuevo Vehículo</button>
      </div>
      <div className="module-page__toolbar">
        <div className="search-box"><Search size={16} />
          <input placeholder="Buscar por placa, marca o modelo..." value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
      </div>
      {loading ? (
        <div className="module-loading"><Loader2 size={24} className="spin" /> Cargando...</div>
      ) : (
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead><tr>
              <th>Código</th><th>Placa</th><th>Marca / Modelo</th><th>Año</th>
              <th>Categoría</th><th>Precio/Día</th><th>Estado</th><th>Localización</th><th>Acciones</th>
            </tr></thead>
            <tbody>
              {filtered.map((v) => (
                <tr key={v.idVehiculo}>
                  <td><code>{v.codigoInterno}</code></td>
                  <td><strong>{v.placa}</strong></td>
                  <td>{v.marca} {v.modelo}</td>
                  <td>{v.anioFabricacion}</td>
                  <td><span className="badge">{v.categoria}</span></td>
                  <td><strong>${Number(v.precioBaseDia).toFixed(2)}</strong></td>
                  <td>
                    {v.estadoOperativo === 'ALQUILADO' ? (
                      <span className="status-badge status-badge--warning">ALQUILADO</span>
                    ) : (
                      <select
                        className="form-input"
                        value={v.estadoOperativo}
                        onChange={(e) => handleEstadoOperativoChange(v, e.target.value)}
                        style={{ minWidth: 150 }}
                      >
                        {ESTADOS_OPERATIVOS_EDITABLES.map((estado) => (
                          <option key={estado} value={estado}>{estado}</option>
                        ))}
                      </select>
                    )}
                  </td>
                  <td>{v.localizacion}</td>
                  <td className="table-actions">
                    <button className="icon-btn" onClick={() => openEdit(v)} title="Editar"><Pencil size={15} /></button>
                    <button className="icon-btn icon-btn--danger" onClick={() => handleDelete(v.idVehiculo)} title="Eliminar"><Trash2 size={15} /></button>
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && <tr><td colSpan={9} className="table-empty">No se encontraron vehículos</td></tr>}
            </tbody>
          </table>
        </div>
      )}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal modal--lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal__header">
              <h2>{editingId ? 'Editar Vehículo' : 'Nuevo Vehículo'}</h2>
              <button className="icon-btn" onClick={() => setShowModal(false)}><X size={18} /></button>
            </div>
            <form onSubmit={handleSave} className="modal__body">
              <div className="form-row form-row--3">
                <div className="form-group"><label className="form-label">Placa</label>
                  <input className="form-input" required value={form.placaVehiculo} onChange={(e) => setForm({...form, placaVehiculo: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Marca</label>
                  <select className="form-input" required value={form.idMarca} onChange={(e) => setForm({...form, idMarca: e.target.value})}>
                    <option value="">Seleccionar</option>
                    {marcas.map(m => <option key={m.idMarcaVehiculo || m.id} value={m.idMarcaVehiculo || m.id}>{m.nombreMarca || m.nombre}</option>)}
                  </select></div>
                <div className="form-group"><label className="form-label">Categoría</label>
                  <select className="form-input" required value={form.idCategoria} onChange={(e) => setForm({...form, idCategoria: e.target.value})}>
                    <option value="">Seleccionar</option>
                    {categorias.map(c => <option key={c.idCategoriaVehiculo || c.id} value={c.idCategoriaVehiculo || c.id}>{c.nombreCategoria || c.nombre}</option>)}
                  </select></div>
              </div>
              <div className="form-row form-row--3">
                <div className="form-group"><label className="form-label">Modelo</label>
                  <input className="form-input" required value={form.modeloVehiculo} onChange={(e) => setForm({...form, modeloVehiculo: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Año</label>
                  <input type="number" className="form-input" required value={form.anioFabricacion} onChange={(e) => setForm({...form, anioFabricacion: Number(e.target.value)})} /></div>
                <div className="form-group"><label className="form-label">Color</label>
                  <input className="form-input" required value={form.colorVehiculo} onChange={(e) => setForm({...form, colorVehiculo: e.target.value})} /></div>
              </div>
              <div className="form-row form-row--3">
                <div className="form-group"><label className="form-label">Combustible</label>
                  <select className="form-input" value={form.tipoCombustible} onChange={(e) => setForm({...form, tipoCombustible: e.target.value})}>
                    <option value="GASOLINA">Gasolina</option><option value="DIESEL">Diesel</option><option value="HIBRIDO">Híbrido</option><option value="ELECTRICO">Eléctrico</option>
                  </select></div>
                <div className="form-group"><label className="form-label">Transmisión</label>
                  <select className="form-input" value={form.tipoTransmision} onChange={(e) => setForm({...form, tipoTransmision: e.target.value})}>
                    <option value="AUTOMATICA">Automática</option><option value="MANUAL">Manual</option>
                  </select></div>
                <div className="form-group"><label className="form-label">Localización</label>
                  <select className="form-input" required value={form.idLocalizacion} onChange={(e) => setForm({...form, idLocalizacion: e.target.value})}>
                    <option value="">Seleccionar</option>
                    {localizaciones.map(l => <option key={l.idLocalizacion || l.id} value={l.idLocalizacion || l.id}>{l.nombreLocalizacion || l.nombre}</option>)}
                  </select></div>
              </div>
              <div className="form-row form-row--4">
                <div className="form-group"><label className="form-label">Pasajeros</label>
                  <input type="number" className="form-input" value={form.capacidadPasajeros} onChange={(e) => setForm({...form, capacidadPasajeros: Number(e.target.value)})} /></div>
                <div className="form-group"><label className="form-label">Maletas</label>
                  <input type="number" className="form-input" value={form.capacidadMaletas} onChange={(e) => setForm({...form, capacidadMaletas: Number(e.target.value)})} /></div>
                <div className="form-group"><label className="form-label">Puertas</label>
                  <input type="number" className="form-input" value={form.numeroPuertas} onChange={(e) => setForm({...form, numeroPuertas: Number(e.target.value)})} /></div>
                <div className="form-group"><label className="form-label">Precio/Día</label>
                  <input type="number" step="0.01" className="form-input" required value={form.precioBaseDia} onChange={(e) => setForm({...form, precioBaseDia: e.target.value})} /></div>
              </div>
              <div className="form-group"><label className="form-label">Kilometraje Actual</label>
                <input type="number" className="form-input" value={form.kilometrajeActual} onChange={(e) => setForm({...form, kilometrajeActual: Number(e.target.value)})} /></div>
              <ImageUploader
                value={form.imagenReferencialUrl}
                onChange={(url) => setForm({...form, imagenReferencialUrl: url})}
                label="Imagen del vehículo"
              />
              <div className="form-group"><label className="form-label">Observaciones</label>
                <input className="form-input" value={form.observacionesGenerales} onChange={(e) => setForm({...form, observacionesGenerales: e.target.value})} /></div>
              <div className="form-group">
                <label className="form-checkbox"><input type="checkbox" checked={form.aireAcondicionado} onChange={(e) => setForm({...form, aireAcondicionado: e.target.checked})} /> Aire acondicionado</label>
              </div>
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
