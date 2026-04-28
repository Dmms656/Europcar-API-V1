import { useEffect, useMemo, useState } from 'react';
import { toast } from 'sonner';
import {
  Plus, Search, X, RefreshCw, Loader2, Edit3, Trash2, ToggleLeft, ToggleRight, PackagePlus,
} from 'lucide-react';
import { catalogosApi } from '../../api/catalogosApi';
import { useAuthStore } from '../../store/useAuthStore';

const ESTADO_FILTERS = [
  { id: 'todas', label: 'Todas' },
  { id: 'activas', label: 'Activas' },
  { id: 'inactivas', label: 'Inactivas' },
];

const TIPO_OPTIONS = ['SERVICIO', 'EQUIPO', 'SEGURO', 'OTRO'];

const initialForm = {
  codigoExtra: '',
  nombreExtra: '',
  descripcionExtra: '',
  tipoExtra: 'SERVICIO',
  requiereStock: false,
  valorFijo: '',
};

function normalizeExtra(raw) {
  if (!raw) return null;
  return {
    idExtra: raw.idExtra ?? raw.id ?? null,
    extraGuid: raw.extraGuid ?? raw.guid ?? null,
    codigoExtra: raw.codigoExtra ?? raw.codigo ?? '',
    nombreExtra: raw.nombreExtra ?? raw.nombre ?? '',
    descripcionExtra: raw.descripcionExtra ?? raw.descripcion ?? '',
    tipoExtra: raw.tipoExtra ?? raw.tipo ?? 'SERVICIO',
    requiereStock: Boolean(raw.requiereStock ?? false),
    valorFijo: Number(raw.valorFijo ?? 0),
    estadoExtra: raw.estadoExtra ?? raw.estado ?? 'ACT',
  };
}

export default function ExtrasPage() {
  const { hasAnyRole } = useAuthStore();
  const isAdmin = hasAnyRole('ADMIN');

  const [extras, setExtras] = useState([]);
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
      const res = await catalogosApi.getExtras();
      const normalized = (res.data?.data || [])
        .map(normalizeExtra)
        .filter(Boolean);
      setExtras(normalized);
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al cargar extras');
    } finally {
      setLoading(false);
    }
  };

  const filtered = useMemo(() => {
    const term = search.trim().toLowerCase();
    return extras.filter((e) => {
      if (estadoFilter === 'activas' && e.estadoExtra !== 'ACT') return false;
      if (estadoFilter === 'inactivas' && e.estadoExtra === 'ACT') return false;
      if (!term) return true;
      return (
        e.codigoExtra?.toLowerCase().includes(term)
        || e.nombreExtra?.toLowerCase().includes(term)
        || e.descripcionExtra?.toLowerCase().includes(term)
        || e.tipoExtra?.toLowerCase().includes(term)
      );
    });
  }, [extras, estadoFilter, search]);

  const stats = useMemo(() => {
    const total = extras.length;
    const activas = extras.filter((e) => e.estadoExtra === 'ACT').length;
    const inactivas = total - activas;
    return { total, activas, inactivas };
  }, [extras]);

  const openCreate = () => {
    if (!isAdmin) return;
    setEditingId(null);
    setForm(initialForm);
    setShowModal(true);
  };

  const openEdit = (extra) => {
    setEditingId(extra.idExtra);
    setForm({
      codigoExtra: extra.codigoExtra || '',
      nombreExtra: extra.nombreExtra || '',
      descripcionExtra: extra.descripcionExtra || '',
      tipoExtra: extra.tipoExtra || 'SERVICIO',
      requiereStock: Boolean(extra.requiereStock),
      valorFijo: String(extra.valorFijo ?? ''),
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
    if (!editingId && !form.codigoExtra.trim()) return 'El código es obligatorio';
    if (!form.nombreExtra.trim()) return 'El nombre es obligatorio';
    if (!form.tipoExtra.trim()) return 'El tipo es obligatorio';
    if (form.valorFijo === '' || Number(form.valorFijo) < 0) return 'El valor fijo debe ser mayor o igual a 0';
    return null;
  };

  const handleSave = async () => {
    const err = validate();
    if (err) { toast.error(err); return; }

    const payload = {
      nombreExtra: form.nombreExtra.trim(),
      descripcionExtra: form.descripcionExtra.trim() || null,
      tipoExtra: form.tipoExtra.trim().toUpperCase(),
      requiereStock: Boolean(form.requiereStock),
      valorFijo: Number(form.valorFijo),
    };

    setSaving(true);
    try {
      if (editingId) {
        await catalogosApi.updateExtra(editingId, payload);
        toast.success('Extra actualizado');
      } else {
        await catalogosApi.createExtra({
          ...payload,
          codigoExtra: form.codigoExtra.trim().toUpperCase(),
        });
        toast.success('Extra creado');
      }
      setShowModal(false);
      setEditingId(null);
      setForm(initialForm);
      await loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al guardar extra');
    } finally {
      setSaving(false);
    }
  };

  const toggleEstado = async (extra) => {
    if (!isAdmin) return;
    const nuevoEstado = extra.estadoExtra === 'ACT' ? 'INA' : 'ACT';
    try {
      await catalogosApi.cambiarEstadoExtra(extra.idExtra, nuevoEstado);
      toast.success(`Extra ${nuevoEstado === 'ACT' ? 'activado' : 'inhabilitado'}`);
      loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al cambiar estado');
    }
  };

  const handleDelete = async (extra) => {
    if (!isAdmin) return;
    if (!confirm(`¿Eliminar el extra "${extra.nombreExtra}"?`)) return;
    try {
      await catalogosApi.deleteExtra(extra.idExtra);
      toast.success('Extra eliminado');
      loadAll();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al eliminar');
    }
  };

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div>
          <h1><PackagePlus size={28} /> Gestión de Extras</h1>
          <p>{stats.total} extras · {stats.activas} activos · {stats.inactivas} inactivos</p>
        </div>
        <div className="module-page__actions">
          <button className="btn btn--outline btn--sm" onClick={loadAll} disabled={loading}>
            <RefreshCw size={16} className={loading ? 'spin' : ''} /> Recargar
          </button>
          {isAdmin && (
            <button className="btn btn--primary" onClick={openCreate}>
              <Plus size={18} /> Nuevo Extra
            </button>
          )}
        </div>
      </div>

      <div className="module-page__toolbar">
        <div className="search-box">
          <Search size={18} />
          <input
            type="text"
            placeholder="Buscar por código, nombre, descripción o tipo..."
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
          <Loader2 size={24} className="spin" /> Cargando extras...
        </div>
      ) : (
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>Código</th>
                <th>Nombre</th>
                <th>Tipo</th>
                <th>Precio (USD)</th>
                <th>Stock</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((e) => (
                <tr key={e.idExtra}>
                  <td><code>{e.codigoExtra}</code></td>
                  <td>
                    <div className="user-cell">
                      <div className="user-cell__avatar"><PackagePlus size={16} /></div>
                      <div>
                        <div className="user-cell__name">{e.nombreExtra}</div>
                        <small style={{ color: 'var(--color-text-muted)' }}>{e.descripcionExtra || '—'}</small>
                      </div>
                    </div>
                  </td>
                  <td>{e.tipoExtra || '—'}</td>
                  <td>{Number(e.valorFijo ?? 0).toFixed(2)}</td>
                  <td>{e.requiereStock ? 'Requiere stock' : 'Sin stock'}</td>
                  <td>
                    <span className={`status-badge status-badge--${e.estadoExtra === 'ACT' ? 'success' : 'danger'}`}>
                      {e.estadoExtra === 'ACT' ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td>
                    <div className="table-actions">
                      <button className="icon-btn" onClick={() => openEdit(e)} title="Editar">
                        <Edit3 size={15} />
                      </button>
                      {isAdmin && (
                        <button
                          className="icon-btn"
                          onClick={() => toggleEstado(e)}
                          title={e.estadoExtra === 'ACT' ? 'Inhabilitar' : 'Activar'}
                        >
                          {e.estadoExtra === 'ACT'
                            ? <ToggleRight size={18} color="var(--color-success)" />
                            : <ToggleLeft size={18} />}
                        </button>
                      )}
                      {isAdmin && (
                        <button
                          className="icon-btn icon-btn--danger"
                          onClick={() => handleDelete(e)}
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
                  <td colSpan={7} className="table-empty">No se encontraron extras</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {showModal && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal modal--lg" onClick={(ev) => ev.stopPropagation()}>
            <div className="modal__header">
              <h2>
                <PackagePlus size={18} style={{ marginRight: 8, verticalAlign: 'text-bottom' }} />
                {editingId ? 'Editar Extra' : 'Nuevo Extra'}
              </h2>
              <button className="icon-btn" onClick={closeModal}><X size={18} /></button>
            </div>
            <div className="modal__body">
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Código *</label>
                  <input
                    className="form-input"
                    value={form.codigoExtra}
                    maxLength={20}
                    placeholder="GPS"
                    disabled={!!editingId}
                    onChange={(e) => setForm({ ...form, codigoExtra: e.target.value.toUpperCase() })}
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
                    value={form.nombreExtra}
                    maxLength={100}
                    placeholder="GPS portátil"
                    onChange={(e) => setForm({ ...form, nombreExtra: e.target.value })}
                  />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Tipo *</label>
                  <select
                    className="form-input"
                    value={form.tipoExtra}
                    onChange={(e) => setForm({ ...form, tipoExtra: e.target.value })}
                  >
                    {TIPO_OPTIONS.map((opt) => (
                      <option key={opt} value={opt}>{opt}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Valor fijo (USD) *</label>
                  <input
                    className="form-input"
                    type="number"
                    min={0}
                    step="0.01"
                    value={form.valorFijo}
                    placeholder="5.00"
                    onChange={(e) => setForm({ ...form, valorFijo: e.target.value })}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Descripción</label>
                <textarea
                  className="form-input"
                  rows={3}
                  maxLength={250}
                  value={form.descripcionExtra}
                  placeholder="Descripción breve del extra"
                  onChange={(e) => setForm({ ...form, descripcionExtra: e.target.value })}
                />
              </div>

              <div className="form-group">
                <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                  <input
                    type="checkbox"
                    checked={form.requiereStock}
                    onChange={(e) => setForm({ ...form, requiereStock: e.target.checked })}
                  />
                  Requiere control de stock por localización
                </label>
              </div>
            </div>
            <div className="modal__footer">
              <button className="btn btn--ghost" onClick={closeModal} disabled={saving}>Cancelar</button>
              <button className="btn btn--primary" onClick={handleSave} disabled={saving}>
                {saving ? (
                  <><Loader2 size={16} className="spin" /> Guardando...</>
                ) : (editingId ? 'Guardar cambios' : 'Crear extra')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
