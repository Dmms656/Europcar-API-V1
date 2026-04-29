import { useEffect, useMemo, useState } from 'react';
import { toast } from 'sonner';
import {
  MapPin, Plus, Search, X, RefreshCw, Loader2, Edit3, Trash2,
  ToggleLeft, ToggleRight, Building2, Phone, Mail, Clock, Map,
} from 'lucide-react';
import { localizacionesApi } from '../../api/localizacionesApi';
import { useAuthStore } from '../../store/useAuthStore';
import { useClientPagination } from '../../hooks/useClientPagination';
import PaginationControls from '../../components/ui/PaginationControls';

const ESTADO_FILTERS = [
  { id: 'todas', label: 'Todas' },
  { id: 'activas', label: 'Activas' },
  { id: 'inactivas', label: 'Inactivas' },
];

const initialForm = {
  codigoLocalizacion: '',
  nombreLocalizacion: '',
  idCiudad: '',
  direccionLocalizacion: '',
  telefonoContacto: '',
  correoContacto: '',
  horarioAtencion: '',
  zonaHoraria: 'America/Guayaquil',
  latitud: '',
  longitud: '',
};

export default function LocalizacionesPage() {
  const { hasAnyRole } = useAuthStore();
  const isAdmin = hasAnyRole('ADMIN');

  const [localizaciones, setLocalizaciones] = useState([]);
  const [ciudades, setCiudades] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [estadoFilter, setEstadoFilter] = useState('todas');

  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState(initialForm);

  useEffect(() => { loadAll(); }, []);

  const loadAll = async () => {
    setLoading(true);
    try {
      const [resLoc, resCiu] = await Promise.all([
        localizacionesApi.getAll(false),
        localizacionesApi.getCiudades(),
      ]);
      setLocalizaciones(resLoc.data?.data || []);
      setCiudades(resCiu.data?.data || []);
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al cargar localizaciones');
    } finally {
      setLoading(false);
    }
  };

  const filtered = useMemo(() => {
    const term = search.trim().toLowerCase();
    return localizaciones.filter((l) => {
      if (estadoFilter === 'activas' && l.estadoLocalizacion !== 'ACT') return false;
      if (estadoFilter === 'inactivas' && l.estadoLocalizacion === 'ACT') return false;
      if (!term) return true;
      return (
        l.codigoLocalizacion?.toLowerCase().includes(term) ||
        l.nombreLocalizacion?.toLowerCase().includes(term) ||
        l.nombreCiudad?.toLowerCase().includes(term) ||
        l.direccionLocalizacion?.toLowerCase().includes(term) ||
        l.correoContacto?.toLowerCase().includes(term)
      );
    });
  }, [localizaciones, estadoFilter, search]);
  const pagination = useClientPagination(filtered, 10);

  const stats = useMemo(() => {
    const total = localizaciones.length;
    const activas = localizaciones.filter((l) => l.estadoLocalizacion === 'ACT').length;
    const inactivas = total - activas;
    return { total, activas, inactivas };
  }, [localizaciones]);

  const openCreate = () => {
    if (!isAdmin) return;
    setEditingId(null);
    setForm(initialForm);
    setShowModal(true);
  };

  const openEdit = (loc) => {
    setEditingId(loc.idLocalizacion);
    setForm({
      codigoLocalizacion: loc.codigoLocalizacion || '',
      nombreLocalizacion: loc.nombreLocalizacion || '',
      idCiudad: loc.idCiudad ? String(loc.idCiudad) : '',
      direccionLocalizacion: loc.direccionLocalizacion || '',
      telefonoContacto: loc.telefonoContacto || '',
      correoContacto: loc.correoContacto || '',
      horarioAtencion: loc.horarioAtencion || '',
      zonaHoraria: loc.zonaHoraria || 'America/Guayaquil',
      latitud: loc.latitud != null ? String(loc.latitud) : '',
      longitud: loc.longitud != null ? String(loc.longitud) : '',
    });
    setShowModal(true);
  };

  const closeModal = () => {
    if (saving) return;
    setShowModal(false);
    setEditingId(null);
    setForm(initialForm);
  };

  const validate = () => {
    if (!editingId && !form.codigoLocalizacion.trim()) return 'El código es obligatorio';
    if (!form.nombreLocalizacion.trim()) return 'El nombre es obligatorio';
    if (!form.idCiudad) return 'Selecciona una ciudad';
    if (!form.direccionLocalizacion.trim()) return 'La dirección es obligatoria';
    if (!form.telefonoContacto.trim()) return 'El teléfono es obligatorio';
    if (!form.correoContacto.trim()) return 'El correo es obligatorio';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.correoContacto)) return 'Correo inválido';
    if (!form.horarioAtencion.trim()) return 'El horario de atención es obligatorio';
    return null;
  };

  const handleSave = async () => {
    const err = validate();
    if (err) { toast.error(err); return; }

    const payload = {
      nombreLocalizacion: form.nombreLocalizacion.trim(),
      idCiudad: Number(form.idCiudad),
      direccionLocalizacion: form.direccionLocalizacion.trim(),
      telefonoContacto: form.telefonoContacto.trim(),
      correoContacto: form.correoContacto.trim(),
      horarioAtencion: form.horarioAtencion.trim(),
      zonaHoraria: form.zonaHoraria || 'America/Guayaquil',
      latitud: form.latitud === '' ? null : Number(form.latitud),
      longitud: form.longitud === '' ? null : Number(form.longitud),
    };

    setSaving(true);
    try {
      if (editingId) {
        await localizacionesApi.update(editingId, payload);
        toast.success('Localización actualizada');
      } else {
        await localizacionesApi.create({
          ...payload,
          codigoLocalizacion: form.codigoLocalizacion.trim().toUpperCase(),
        });
        toast.success('Localización creada');
      }
      setShowModal(false);
      setEditingId(null);
      setForm(initialForm);
      await loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al guardar la localización');
    } finally {
      setSaving(false);
    }
  };

  const toggleEstado = async (loc) => {
    if (!isAdmin) return;
    const nuevoEstado = loc.estadoLocalizacion === 'ACT' ? 'INA' : 'ACT';
    try {
      await localizacionesApi.cambiarEstado(loc.idLocalizacion, nuevoEstado);
      toast.success(`Localización ${nuevoEstado === 'ACT' ? 'activada' : 'inhabilitada'}`);
      loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al cambiar el estado');
    }
  };

  const handleDelete = async (loc) => {
    if (!isAdmin) return;
    if (!confirm(`¿Eliminar la localización "${loc.nombreLocalizacion}"? Esta acción la marcará como inhabilitada.`)) return;
    try {
      await localizacionesApi.delete(loc.idLocalizacion);
      toast.success('Localización eliminada');
      loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al eliminar');
    }
  };

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div>
          <h1><MapPin size={28} /> Gestión de Localizaciones</h1>
          <p>{stats.total} sucursales · {stats.activas} activas · {stats.inactivas} inactivas</p>
        </div>
        <div className="module-page__actions">
          <button className="btn btn--outline btn--sm" onClick={loadAll} disabled={loading}>
            <RefreshCw size={16} className={loading ? 'spin' : ''} /> Recargar
          </button>
          {isAdmin && (
            <button className="btn btn--primary" onClick={openCreate}>
              <Plus size={18} /> Nueva Localización
            </button>
          )}
        </div>
      </div>

      <div className="module-page__toolbar">
        <div className="search-box">
          <Search size={18} />
          <input
            type="text"
            placeholder="Buscar por código, nombre, ciudad, dirección o correo..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
        <div className="filter-tabs">
          {ESTADO_FILTERS.map((f) => (
            <button
              key={f.id}
              type="button"
              className={`filter-tab ${estadoFilter === f.id ? 'filter-tab--active' : ''}`}
              onClick={() => setEstadoFilter(f.id)}
            >
              {f.label}
            </button>
          ))}
        </div>
      </div>

      {loading ? (
        <div className="module-loading">
          <Loader2 size={24} className="spin" /> Cargando localizaciones...
        </div>
      ) : (
        <>
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>Código</th>
                <th>Sucursal</th>
                <th>Ciudad</th>
                <th>Contacto</th>
                <th>Horario</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {pagination.paginatedItems.map((l) => (
                <tr key={l.idLocalizacion}>
                  <td><code>{l.codigoLocalizacion}</code></td>
                  <td>
                    <div className="user-cell">
                      <div className="user-cell__avatar"><Building2 size={16} /></div>
                      <div>
                        <div className="user-cell__name">{l.nombreLocalizacion}</div>
                        <small style={{ color: 'var(--color-text-muted)' }}>{l.direccionLocalizacion}</small>
                      </div>
                    </div>
                  </td>
                  <td>{l.nombreCiudad || '—'}</td>
                  <td>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: 2, fontSize: '0.78rem' }}>
                      <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
                        <Phone size={12} /> {l.telefonoContacto || '—'}
                      </span>
                      <span style={{ display: 'flex', alignItems: 'center', gap: 4, color: 'var(--color-text-muted)' }}>
                        <Mail size={12} /> {l.correoContacto || '—'}
                      </span>
                    </div>
                  </td>
                  <td>
                    <span style={{ display: 'inline-flex', alignItems: 'center', gap: 4, fontSize: '0.78rem' }}>
                      <Clock size={12} /> {l.horarioAtencion || '—'}
                    </span>
                  </td>
                  <td>
                    <span className={`status-badge status-badge--${l.estadoLocalizacion === 'ACT' ? 'success' : 'danger'}`}>
                      {l.estadoLocalizacion === 'ACT' ? 'Activa' : 'Inactiva'}
                    </span>
                  </td>
                  <td>
                    <div className="table-actions">
                      <button className="icon-btn" onClick={() => openEdit(l)} title="Editar">
                        <Edit3 size={15} />
                      </button>
                      {isAdmin && (
                        <button
                          className="icon-btn"
                          onClick={() => toggleEstado(l)}
                          title={l.estadoLocalizacion === 'ACT' ? 'Inhabilitar' : 'Activar'}
                        >
                          {l.estadoLocalizacion === 'ACT'
                            ? <ToggleRight size={18} color="var(--color-success)" />
                            : <ToggleLeft size={18} />}
                        </button>
                      )}
                      {isAdmin && (
                        <button
                          className="icon-btn icon-btn--danger"
                          onClick={() => handleDelete(l)}
                          title="Eliminar"
                        >
                          <Trash2 size={15} />
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && (
                <tr>
                  <td colSpan={7} className="table-empty">No se encontraron localizaciones</td>
                </tr>
              )}
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
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal modal--lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal__header">
              <h2>
                <MapPin size={18} style={{ marginRight: 8, verticalAlign: 'text-bottom' }} />
                {editingId ? 'Editar Localización' : 'Nueva Localización'}
              </h2>
              <button className="icon-btn" onClick={closeModal}><X size={18} /></button>
            </div>
            <div className="modal__body">
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Código *</label>
                  <input
                    className="form-input"
                    value={form.codigoLocalizacion}
                    placeholder="UIO-MARISCAL"
                    maxLength={20}
                    disabled={!!editingId}
                    onChange={(e) => setForm({ ...form, codigoLocalizacion: e.target.value.toUpperCase() })}
                  />
                  {editingId && (
                    <small style={{ color: 'var(--color-text-muted)' }}>
                      El código no se puede modificar.
                    </small>
                  )}
                </div>
                <div className="form-group">
                  <label className="form-label">Nombre *</label>
                  <input
                    className="form-input"
                    value={form.nombreLocalizacion}
                    placeholder="Sucursal La Mariscal"
                    maxLength={100}
                    onChange={(e) => setForm({ ...form, nombreLocalizacion: e.target.value })}
                  />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label className="form-label"><Map size={13} /> Ciudad *</label>
                  <select
                    className="form-input"
                    value={form.idCiudad}
                    onChange={(e) => setForm({ ...form, idCiudad: e.target.value })}
                  >
                    <option value="">— Selecciona una ciudad —</option>
                    {ciudades.map((c) => (
                      <option key={c.idCiudad} value={c.idCiudad}>
                        {c.nombreCiudad}{c.nombrePais ? ` · ${c.nombrePais}` : ''}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Zona horaria</label>
                  <input
                    className="form-input"
                    value={form.zonaHoraria}
                    placeholder="America/Guayaquil"
                    maxLength={50}
                    onChange={(e) => setForm({ ...form, zonaHoraria: e.target.value })}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Dirección *</label>
                <input
                  className="form-input"
                  value={form.direccionLocalizacion}
                  placeholder="Av. Amazonas N34-451 y Atahualpa"
                  maxLength={200}
                  onChange={(e) => setForm({ ...form, direccionLocalizacion: e.target.value })}
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label className="form-label"><Phone size={13} /> Teléfono *</label>
                  <input
                    className="form-input"
                    value={form.telefonoContacto}
                    placeholder="+593 2 222 3344"
                    maxLength={20}
                    onChange={(e) => setForm({ ...form, telefonoContacto: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label"><Mail size={13} /> Correo *</label>
                  <input
                    className="form-input"
                    type="email"
                    value={form.correoContacto}
                    placeholder="sucursal@europcar.ec"
                    maxLength={120}
                    onChange={(e) => setForm({ ...form, correoContacto: e.target.value })}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label"><Clock size={13} /> Horario de atención *</label>
                <input
                  className="form-input"
                  value={form.horarioAtencion}
                  placeholder="Lun-Vie 08:00-18:00 / Sáb 09:00-13:00"
                  maxLength={120}
                  onChange={(e) => setForm({ ...form, horarioAtencion: e.target.value })}
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Latitud</label>
                  <input
                    className="form-input"
                    type="number"
                    step="0.000001"
                    min={-90}
                    max={90}
                    value={form.latitud}
                    placeholder="-0.180653"
                    onChange={(e) => setForm({ ...form, latitud: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Longitud</label>
                  <input
                    className="form-input"
                    type="number"
                    step="0.000001"
                    min={-180}
                    max={180}
                    value={form.longitud}
                    placeholder="-78.467838"
                    onChange={(e) => setForm({ ...form, longitud: e.target.value })}
                  />
                </div>
              </div>
            </div>
            <div className="modal__footer">
              <button className="btn btn--ghost" onClick={closeModal} disabled={saving}>Cancelar</button>
              <button className="btn btn--primary" onClick={handleSave} disabled={saving}>
                {saving ? (
                  <><Loader2 size={16} className="spin" /> Guardando...</>
                ) : (editingId ? 'Guardar cambios' : 'Crear localización')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
