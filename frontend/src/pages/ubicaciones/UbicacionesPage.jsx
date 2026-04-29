import { useEffect, useMemo, useState } from 'react';
import { toast } from 'sonner';
import {
  Globe, Building2, Plus, RefreshCw, Loader2, Edit3, Trash2, ToggleLeft, ToggleRight, X,
} from 'lucide-react';
import { catalogosApi } from '../../api/catalogosApi';
import { useAuthStore } from '../../store/useAuthStore';
import { useClientPagination } from '../../hooks/useClientPagination';
import PaginationControls from '../../components/ui/PaginationControls';

const initialPaisForm = { codigoIso2: '', nombrePais: '' };
const initialCiudadForm = { idPais: '', nombreCiudad: '' };

export default function UbicacionesPage() {
  const { hasAnyRole } = useAuthStore();
  const isAdmin = hasAnyRole('ADMIN');

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [paises, setPaises] = useState([]);
  const [ciudades, setCiudades] = useState([]);

  const [showPaisModal, setShowPaisModal] = useState(false);
  const [editingPaisId, setEditingPaisId] = useState(null);
  const [paisForm, setPaisForm] = useState(initialPaisForm);

  const [showCiudadModal, setShowCiudadModal] = useState(false);
  const [editingCiudadId, setEditingCiudadId] = useState(null);
  const [ciudadForm, setCiudadForm] = useState(initialCiudadForm);

  useEffect(() => { loadAll(); }, []);

  const loadAll = async () => {
    setLoading(true);
    try {
      const [resPaises, resCiudades] = await Promise.all([
        catalogosApi.getPaises(),
        catalogosApi.getCiudades(),
      ]);
      setPaises(resPaises.data?.data || []);
      setCiudades(resCiudades.data?.data || []);
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al cargar países y ciudades');
    } finally {
      setLoading(false);
    }
  };

  const paisStats = useMemo(() => {
    const total = paises.length;
    const activos = paises.filter((p) => p.estado === 'ACT').length;
    return { total, activos, inactivos: total - activos };
  }, [paises]);

  const ciudadStats = useMemo(() => {
    const total = ciudades.length;
    const activos = ciudades.filter((c) => c.estadoCiudad === 'ACT').length;
    return { total, activos, inactivos: total - activos };
  }, [ciudades]);
  const paisPagination = useClientPagination(paises, 10);
  const ciudadPagination = useClientPagination(ciudades, 10);

  const openPaisCreate = () => {
    if (!isAdmin) return;
    setEditingPaisId(null);
    setPaisForm(initialPaisForm);
    setShowPaisModal(true);
  };

  const openPaisEdit = (pais) => {
    setEditingPaisId(pais.id);
    setPaisForm({
      codigoIso2: pais.codigo || '',
      nombrePais: pais.nombre || '',
    });
    setShowPaisModal(true);
  };

  const savePais = async () => {
    if (!paisForm.nombrePais.trim()) return toast.error('El nombre del país es obligatorio');
    if (!editingPaisId && !/^[A-Za-z]{2}$/.test(paisForm.codigoIso2.trim())) {
      return toast.error('El código ISO2 debe tener 2 letras');
    }
    setSaving(true);
    try {
      if (editingPaisId) {
        await catalogosApi.updatePais(editingPaisId, { nombrePais: paisForm.nombrePais.trim() });
        toast.success('País actualizado');
      } else {
        await catalogosApi.createPais({
          codigoIso2: paisForm.codigoIso2.trim().toUpperCase(),
          nombrePais: paisForm.nombrePais.trim(),
        });
        toast.success('País creado');
      }
      setShowPaisModal(false);
      await loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al guardar país');
    } finally {
      setSaving(false);
    }
  };

  const togglePaisEstado = async (pais) => {
    const estado = pais.estado === 'ACT' ? 'INA' : 'ACT';
    try {
      await catalogosApi.cambiarEstadoPais(pais.id, estado);
      toast.success(`País ${estado === 'ACT' ? 'activado' : 'inhabilitado'}`);
      loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al cambiar estado de país');
    }
  };

  const deletePais = async (pais) => {
    if (!confirm(`¿Eliminar el país "${pais.nombre}"?`)) return;
    try {
      await catalogosApi.deletePais(pais.id);
      toast.success('País eliminado');
      loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al eliminar país');
    }
  };

  const openCiudadCreate = () => {
    if (!isAdmin) return;
    setEditingCiudadId(null);
    setCiudadForm(initialCiudadForm);
    setShowCiudadModal(true);
  };

  const openCiudadEdit = (ciudad) => {
    setEditingCiudadId(ciudad.idCiudad);
    setCiudadForm({
      idPais: String(ciudad.idPais || ''),
      nombreCiudad: ciudad.nombreCiudad || '',
    });
    setShowCiudadModal(true);
  };

  const saveCiudad = async () => {
    if (!ciudadForm.idPais) return toast.error('Selecciona un país');
    if (!ciudadForm.nombreCiudad.trim()) return toast.error('El nombre de la ciudad es obligatorio');
    setSaving(true);
    try {
      const payload = { idPais: Number(ciudadForm.idPais), nombreCiudad: ciudadForm.nombreCiudad.trim() };
      if (editingCiudadId) {
        await catalogosApi.updateCiudad(editingCiudadId, payload);
        toast.success('Ciudad actualizada');
      } else {
        await catalogosApi.createCiudad(payload);
        toast.success('Ciudad creada');
      }
      setShowCiudadModal(false);
      await loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al guardar ciudad');
    } finally {
      setSaving(false);
    }
  };

  const toggleCiudadEstado = async (ciudad) => {
    const estado = ciudad.estadoCiudad === 'ACT' ? 'INA' : 'ACT';
    try {
      await catalogosApi.cambiarEstadoCiudad(ciudad.idCiudad, estado);
      toast.success(`Ciudad ${estado === 'ACT' ? 'activada' : 'inhabilitada'}`);
      loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al cambiar estado de ciudad');
    }
  };

  const deleteCiudad = async (ciudad) => {
    if (!confirm(`¿Eliminar la ciudad "${ciudad.nombreCiudad}"?`)) return;
    try {
      await catalogosApi.deleteCiudad(ciudad.idCiudad);
      toast.success('Ciudad eliminada');
      loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al eliminar ciudad');
    }
  };

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div>
          <h1><Globe size={28} /> Gestión de Países y Ciudades</h1>
          <p>{paisStats.total} países ({paisStats.activos} activos) · {ciudadStats.total} ciudades ({ciudadStats.activos} activas)</p>
        </div>
        <div className="module-page__actions">
          <button className="btn btn--outline btn--sm" onClick={loadAll} disabled={loading}>
            <RefreshCw size={16} className={loading ? 'spin' : ''} /> Recargar
          </button>
          {isAdmin && (
            <>
              <button className="btn btn--outline" onClick={openPaisCreate}><Plus size={16} /> Nuevo País</button>
              <button className="btn btn--primary" onClick={openCiudadCreate}><Plus size={16} /> Nueva Ciudad</button>
            </>
          )}
        </div>
      </div>

      {loading ? (
        <div className="module-loading"><Loader2 size={24} className="spin" /> Cargando catálogo...</div>
      ) : (
        <div style={{ display: 'grid', gap: 16 }}>
          <div className="data-table-wrapper">
            <h3 style={{ margin: '10px 12px' }}><Globe size={16} style={{ verticalAlign: 'text-bottom' }} /> Países</h3>
            <table className="data-table">
              <thead><tr><th>Código</th><th>Nombre</th><th>Estado</th><th>Acciones</th></tr></thead>
              <tbody>
                {paisPagination.paginatedItems.map((p) => (
                  <tr key={p.id}>
                    <td><code>{p.codigo}</code></td>
                    <td>{p.nombre}</td>
                    <td><span className={`status-badge status-badge--${p.estado === 'ACT' ? 'success' : 'danger'}`}>{p.estado === 'ACT' ? 'Activo' : 'Inactivo'}</span></td>
                    <td>
                      <div className="table-actions">
                        <button className="icon-btn" onClick={() => openPaisEdit(p)} title="Editar"><Edit3 size={15} /></button>
                        <button className="icon-btn" onClick={() => togglePaisEstado(p)} title="Cambiar estado">
                          {p.estado === 'ACT' ? <ToggleRight size={18} color="var(--color-success)" /> : <ToggleLeft size={18} />}
                        </button>
                        <button className="icon-btn icon-btn--danger" onClick={() => deletePais(p)} title="Eliminar"><Trash2 size={15} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <PaginationControls
              page={paisPagination.page}
              totalPages={paisPagination.totalPages}
              pageSize={paisPagination.pageSize}
              onPageChange={paisPagination.setPage}
              onPageSizeChange={paisPagination.setPageSize}
              totalItems={paisPagination.totalItems}
              startItem={paisPagination.startItem}
              endItem={paisPagination.endItem}
            />
          </div>

          <div className="data-table-wrapper">
            <h3 style={{ margin: '10px 12px' }}><Building2 size={16} style={{ verticalAlign: 'text-bottom' }} /> Ciudades</h3>
            <table className="data-table">
              <thead><tr><th>Ciudad</th><th>País</th><th>Estado</th><th>Acciones</th></tr></thead>
              <tbody>
                {ciudadPagination.paginatedItems.map((c) => (
                  <tr key={c.idCiudad}>
                    <td>{c.nombreCiudad}</td>
                    <td>{c.nombrePais || `ID ${c.idPais}`}</td>
                    <td><span className={`status-badge status-badge--${c.estadoCiudad === 'ACT' ? 'success' : 'danger'}`}>{c.estadoCiudad === 'ACT' ? 'Activa' : 'Inactiva'}</span></td>
                    <td>
                      <div className="table-actions">
                        <button className="icon-btn" onClick={() => openCiudadEdit(c)} title="Editar"><Edit3 size={15} /></button>
                        <button className="icon-btn" onClick={() => toggleCiudadEstado(c)} title="Cambiar estado">
                          {c.estadoCiudad === 'ACT' ? <ToggleRight size={18} color="var(--color-success)" /> : <ToggleLeft size={18} />}
                        </button>
                        <button className="icon-btn icon-btn--danger" onClick={() => deleteCiudad(c)} title="Eliminar"><Trash2 size={15} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <PaginationControls
              page={ciudadPagination.page}
              totalPages={ciudadPagination.totalPages}
              pageSize={ciudadPagination.pageSize}
              onPageChange={ciudadPagination.setPage}
              onPageSizeChange={ciudadPagination.setPageSize}
              totalItems={ciudadPagination.totalItems}
              startItem={ciudadPagination.startItem}
              endItem={ciudadPagination.endItem}
            />
          </div>
        </div>
      )}

      {showPaisModal && (
        <div className="modal-overlay" onClick={() => !saving && setShowPaisModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal__header">
              <h2>{editingPaisId ? 'Editar País' : 'Nuevo País'}</h2>
              <button className="icon-btn" onClick={() => !saving && setShowPaisModal(false)}><X size={18} /></button>
            </div>
            <div className="modal__body">
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Código ISO2 *</label>
                  <input className="form-input" value={paisForm.codigoIso2} disabled={!!editingPaisId} maxLength={2} onChange={(e) => setPaisForm({ ...paisForm, codigoIso2: e.target.value.toUpperCase() })} />
                </div>
                <div className="form-group">
                  <label className="form-label">Nombre *</label>
                  <input className="form-input" value={paisForm.nombrePais} maxLength={100} onChange={(e) => setPaisForm({ ...paisForm, nombrePais: e.target.value })} />
                </div>
              </div>
            </div>
            <div className="modal__footer">
              <button className="btn btn--ghost" onClick={() => setShowPaisModal(false)} disabled={saving}>Cancelar</button>
              <button className="btn btn--primary" onClick={savePais} disabled={saving}>{saving ? 'Guardando...' : 'Guardar'}</button>
            </div>
          </div>
        </div>
      )}

      {showCiudadModal && (
        <div className="modal-overlay" onClick={() => !saving && setShowCiudadModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal__header">
              <h2>{editingCiudadId ? 'Editar Ciudad' : 'Nueva Ciudad'}</h2>
              <button className="icon-btn" onClick={() => !saving && setShowCiudadModal(false)}><X size={18} /></button>
            </div>
            <div className="modal__body">
              <div className="form-group">
                <label className="form-label">País *</label>
                <select className="form-input" value={ciudadForm.idPais} onChange={(e) => setCiudadForm({ ...ciudadForm, idPais: e.target.value })}>
                  <option value="">Seleccione un país</option>
                  {paises.filter((p) => p.estado === 'ACT').map((p) => (
                    <option key={p.id} value={p.id}>{p.nombre} ({p.codigo})</option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label className="form-label">Nombre de ciudad *</label>
                <input className="form-input" value={ciudadForm.nombreCiudad} maxLength={120} onChange={(e) => setCiudadForm({ ...ciudadForm, nombreCiudad: e.target.value })} />
              </div>
            </div>
            <div className="modal__footer">
              <button className="btn btn--ghost" onClick={() => setShowCiudadModal(false)} disabled={saving}>Cancelar</button>
              <button className="btn btn--primary" onClick={saveCiudad} disabled={saving}>{saving ? 'Guardando...' : 'Guardar'}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
