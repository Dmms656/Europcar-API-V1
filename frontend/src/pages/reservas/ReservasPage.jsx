import { useState, useEffect } from 'react';
import { reservasApi } from '../../api/reservasApi';
import { vehiculosApi } from '../../api/vehiculosApi';
import { clientesApi } from '../../api/clientesApi';
import { catalogosApi } from '../../api/catalogosApi';
import { toast } from 'sonner';
import { Plus, Search, CheckCircle, XCircle, Loader2, CalendarCheck, X, RefreshCw, Pencil } from 'lucide-react';
import { useClientPagination } from '../../hooks/useClientPagination';
import PaginationControls from '../../components/ui/PaginationControls';
import DateTimePicker from '../../components/ui/DateTimePicker';

const EXTRA_CONDUCTOR_ADICIONAL_CODE = 'COND-ADIC';

function normalizeExtra(raw) {
  if (!raw) return null;
  return {
    idExtra: raw.idExtra ?? raw.id ?? null,
    nombreExtra: raw.nombreExtra ?? raw.nombre ?? '',
    descripcionExtra: raw.descripcionExtra ?? raw.descripcion ?? '',
    codigoExtra: raw.codigoExtra ?? raw.codigo ?? '',
    tipoExtra: raw.tipoExtra ?? raw.tipo ?? 'SERVICIO',
    requiereStock: Boolean(raw.requiereStock ?? false),
    valorFijo: Number(raw.valorFijo ?? raw.valor ?? 0),
    estadoExtra: raw.estadoExtra ?? raw.estado ?? 'ACT',
  };
}

function isConductorExtra(extra) {
  const code = String(extra?.codigoExtra || '').toUpperCase();
  return code === EXTRA_CONDUCTOR_ADICIONAL_CODE;
}

export default function ReservasPage() {
  const [reservas, setReservas] = useState([]);
  const [clientes, setClientes] = useState([]);
  const [vehiculos, setVehiculos] = useState([]);
  const [localizaciones, setLocalizaciones] = useState([]);
  const [ciudades, setCiudades] = useState([]);
  const [extras, setExtras] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingReserva, setEditingReserva] = useState(null);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({
    idCliente: '', idVehiculo: '', idLocalizacionRecogida: '', idLocalizacionDevolucion: '',
    canalReserva: 'WEB', fechaHoraRecogida: '', fechaHoraDevolucion: '', extras: [],
  });
  const [conductores, setConductores] = useState([]);
  const conductorExtra = extras.find((e) => isConductorExtra(e));
  const conductoresAdicionalesValidos = conductores.filter(
    (c) => c.numeroIdentificacion && c.nombre1 && c.apellido1 && c.numeroLicencia
  ).length;
  const pagination = useClientPagination(reservas, 10);

  useEffect(() => { loadAll(); }, []);

  const loadAll = async () => {
    setLoading(true);
    try {
      const [cRes, vRes, lRes, eRes, ciRes] = await Promise.all([
        clientesApi.getAll(), vehiculosApi.getAll(),
        catalogosApi.getLocalizaciones(), catalogosApi.getExtras(),
        catalogosApi.getCiudades(),
      ]);
      const clientesList = cRes.data?.data || [];
      setClientes(clientesList);
      setVehiculos(vRes.data?.data || []);
      setLocalizaciones(lRes.data?.data || []);
      const rawExtras = eRes.data?.data || [];
      setExtras(Array.isArray(rawExtras) ? rawExtras.map(normalizeExtra).filter(Boolean) : []);
      setCiudades(ciRes.data?.data || []);

      // Auto-load reservas for all clients (sin límite artificial de 20)
      const responses = await Promise.allSettled(
        clientesList.map((c) => reservasApi.getByCliente(c.idCliente))
      );

      const allReservas = [];
      const seenIds = new Set();
      responses.forEach((result) => {
        if (result.status !== 'fulfilled') return;
        const data = result.value?.data?.data || [];
        data.forEach((r) => {
          if (!seenIds.has(r.idReserva)) {
            seenIds.add(r.idReserva);
            allReservas.push(r);
          }
        });
      });

      setReservas(allReservas);
    } catch (e) { toast.error('Error al cargar datos'); }
    finally { setLoading(false); }
  };

  // Replicar el comportamiento del flujo de cliente:
  // - El extra COND-ADIC se agrega automáticamente cuando hay conductores adicionales.
  // - Su cantidad queda bloqueada en: cantidad de conductores adicionales.
  // - Si no hay conductores adicionales, se elimina.
  useEffect(() => {
    if (editingReserva) return; // para no pisar la lógica de edición (y porque openEdit no carga conductores/extras)
    if (!conductorExtra?.idExtra) return;

    const condId = String(conductorExtra.idExtra);
    const desiredQty = conductoresAdicionalesValidos;

    setForm((prev) => {
      const otherExtras = prev.extras.filter((ex) => String(ex.idExtra) !== condId);
      const already = prev.extras.find((ex) => String(ex.idExtra) === condId);

      if (desiredQty <= 0) {
        if (!already) return prev;
        return { ...prev, extras: otherExtras };
      }

      const nextQty = desiredQty;
      if (already && Number(already.cantidad) === nextQty) return prev;

      return {
        ...prev,
        extras: [
          ...otherExtras,
          {
            idExtra: conductorExtra.idExtra,
            cantidad: nextQty,
          },
        ],
      };
    });
  }, [conductoresAdicionalesValidos, conductorExtra?.idExtra, editingReserva]);

  const buscarReserva = async () => {
    if (!search.trim()) return;
    setLoading(true);
    try {
      const res = await reservasApi.getByCodigo(search.trim());
      const data = res.data?.data;
      setReservas(data ? [data] : []);
      if (!data) toast.info('No se encontró la reserva');
    } catch (e) {
      toast.error('Reserva no encontrada');
      setReservas([]);
    } finally { setLoading(false); }
  };

  const buscarPorCliente = async (idCliente) => {
    if (!idCliente) { loadAll(); return; }
    setLoading(true);
    try {
      const res = await reservasApi.getByCliente(idCliente);
      setReservas(res.data?.data || []);
    } catch (e) { toast.error('Error buscando reservas'); setReservas([]); }
    finally { setLoading(false); }
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (!form.fechaHoraRecogida || !form.fechaHoraDevolucion) {
        throw new Error('Selecciona fecha y hora de recogida y devolución.');
      }
      if (new Date(form.fechaHoraDevolucion) <= new Date(form.fechaHoraRecogida)) {
        throw new Error('La devolución debe ser posterior a la recogida.');
      }

      const idPaisRecogida = getPaisByLocalizacion(form.idLocalizacionRecogida);
      const idPaisDevolucion = getPaisByLocalizacion(form.idLocalizacionDevolucion);
      if (idPaisRecogida && idPaisDevolucion && idPaisRecogida !== idPaisDevolucion) {
        throw new Error('La recogida y devolución deben ser dentro del mismo país.');
      }

      const payload = {
        ...form, idCliente: Number(form.idCliente), idVehiculo: Number(form.idVehiculo),
        idLocalizacionRecogida: Number(form.idLocalizacionRecogida),
        idLocalizacionDevolucion: Number(form.idLocalizacionDevolucion),
        fechaHoraRecogida: new Date(form.fechaHoraRecogida).toISOString(),
        fechaHoraDevolucion: new Date(form.fechaHoraDevolucion).toISOString(),
        extras: form.extras.filter(ex => ex.idExtra && ex.cantidad > 0).map(ex => ({ idExtra: Number(ex.idExtra), cantidad: Number(ex.cantidad) })),
        conductores: [
          { usarClienteTitular: true, esPrincipal: true },
          ...conductores
            .filter(c => c.numeroIdentificacion && c.nombre1 && c.apellido1 && c.numeroLicencia)
            .map(c => ({
              usarClienteTitular: false,
              esPrincipal: false,
              tipoIdentificacion: c.tipoIdentificacion || 'CED',
              numeroIdentificacion: c.numeroIdentificacion.trim(),
              nombre1: c.nombre1.trim(),
              apellido1: c.apellido1.trim(),
              numeroLicencia: c.numeroLicencia.trim().toUpperCase(),
              edadConductor: Number(c.edadConductor || 25),
              telefono: c.telefono?.trim() || '',
              correo: c.correo?.trim() || '',
            })),
        ],
      };
      const res = await reservasApi.create(payload);
      toast.success(`Reserva creada: ${res.data?.data?.codigoReserva || 'OK'}`);
      setShowModal(false);
      setConductores([]);
      loadAll();
    } catch (e) { toast.error(e.response?.data?.message || 'Error al crear reserva'); }
    finally { setSaving(false); }
  };

  const openEdit = (r) => {
    if (r.estadoReserva !== 'PENDIENTE') {
      toast.info('Solo las reservas pendientes pueden editarse.');
      return;
    }
    setEditingReserva(r);
    setForm({
      idCliente: String(r.idCliente || ''),
      idVehiculo: String(r.idVehiculo || ''),
      idLocalizacionRecogida: String(r.idLocalizacionRecogida || ''),
      idLocalizacionDevolucion: String(r.idLocalizacionDevolucion || ''),
      canalReserva: r.canalReserva || 'WEB',
      fechaHoraRecogida: r.fechaHoraRecogida ? new Date(r.fechaHoraRecogida).toISOString().slice(0, 16) : '',
      fechaHoraDevolucion: r.fechaHoraDevolucion ? new Date(r.fechaHoraDevolucion).toISOString().slice(0, 16) : '',
      extras: [],
    });
    setShowModal(true);
  };

  const handleSave = async (e) => {
    if (!editingReserva) return handleCreate(e);
    e.preventDefault();
    setSaving(true);
    try {
      if (!form.fechaHoraRecogida || !form.fechaHoraDevolucion) {
        throw new Error('Selecciona fecha y hora de recogida y devolución.');
      }
      if (new Date(form.fechaHoraDevolucion) <= new Date(form.fechaHoraRecogida)) {
        throw new Error('La devolución debe ser posterior a la recogida.');
      }

      const idPaisRecogida = getPaisByLocalizacion(form.idLocalizacionRecogida);
      const idPaisDevolucion = getPaisByLocalizacion(form.idLocalizacionDevolucion);
      if (idPaisRecogida && idPaisDevolucion && idPaisRecogida !== idPaisDevolucion) {
        throw new Error('La recogida y devolución deben ser dentro del mismo país.');
      }

      const payload = {
        idVehiculo: Number(form.idVehiculo),
        idLocalizacionRecogida: Number(form.idLocalizacionRecogida),
        idLocalizacionDevolucion: Number(form.idLocalizacionDevolucion),
        fechaHoraRecogida: new Date(form.fechaHoraRecogida).toISOString(),
        fechaHoraDevolucion: new Date(form.fechaHoraDevolucion).toISOString(),
        canalReserva: form.canalReserva,
      };
      await reservasApi.update(editingReserva.idReserva, payload);
      toast.success('Reserva actualizada');
      setShowModal(false);
      setEditingReserva(null);
      loadAll();
    } catch (e) { toast.error(e.response?.data?.message || 'Error al actualizar reserva'); }
    finally { setSaving(false); }
  };

  const confirmar = async (id) => {
    try { await reservasApi.confirmar(id); toast.success('Reserva confirmada'); loadAll(); }
    catch (e) { toast.error(e.response?.data?.message || 'Error'); }
  };

  const cancelar = async (id) => {
    if (!confirm('¿Cancelar esta reserva?')) return;
    try { await reservasApi.cancelar(id, 'Cancelado desde panel'); toast.success('Reserva cancelada'); loadAll(); }
    catch (e) { toast.error(e.response?.data?.message || 'Error'); }
  };

  const addExtra = () => setForm({ ...form, extras: [...form.extras, { idExtra: '', cantidad: 1 }] });
  const removeExtra = (i) => setForm({ ...form, extras: form.extras.filter((_, idx) => idx !== i) });
  const addConductor = () => setConductores((prev) => [...prev, {
    tipoIdentificacion: 'CED',
    numeroIdentificacion: '',
    nombre1: '',
    apellido1: '',
    numeroLicencia: '',
    edadConductor: '25',
    telefono: '',
    correo: '',
  }]);
  const removeConductor = (i) => setConductores((prev) => prev.filter((_, idx) => idx !== i));

  const getPaisByLocalizacion = (idLocalizacion) => {
    const loc = localizaciones.find((l) => Number(l.idLocalizacion || l.id) === Number(idLocalizacion));
    if (!loc) return null;
    const ciudad = ciudades.find((c) => Number(c.idCiudad) === Number(loc.idCiudad));
    return ciudad?.idPais ?? null;
  };

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div><h1><CalendarCheck size={24} /> Reservas</h1><p>{reservas.length} reservas encontradas</p></div>
        <div className="module-page__actions">
          <button className="btn btn--outline btn--sm" onClick={loadAll}><RefreshCw size={16} /> Recargar</button>
          <button
            className="btn btn--primary"
            onClick={() => {
              setEditingReserva(null);
              setConductores([]);
              setForm({
                idCliente: '',
                idVehiculo: '',
                idLocalizacionRecogida: '',
                idLocalizacionDevolucion: '',
                canalReserva: 'WEB',
                fechaHoraRecogida: '',
                fechaHoraDevolucion: '',
                extras: [],
              });
              setShowModal(true);
            }}
          >
            <Plus size={16} /> Nueva Reserva
          </button>
        </div>
      </div>
      <div className="module-page__toolbar">
        <div className="search-box"><Search size={16} />
          <input placeholder="Buscar por código de reserva..." value={search} onChange={(e) => setSearch(e.target.value)} onKeyDown={(e) => e.key === 'Enter' && buscarReserva()} />
        </div>
        <select className="form-input" style={{maxWidth:250}} onChange={(e) => buscarPorCliente(e.target.value)}>
          <option value="">Todos los clientes</option>
          {clientes.map(c => <option key={c.idCliente} value={c.idCliente}>{c.nombreCompleto}</option>)}
        </select>
      </div>
      {loading ? (
        <div className="module-loading"><Loader2 size={24} className="spin" /> Cargando reservas...</div>
      ) : reservas.length === 0 ? (
        <div className="module-loading">No se encontraron reservas.</div>
      ) : (
        <>
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead><tr>
              <th>Código</th><th>Cliente</th><th>Vehículo</th><th>Recogida</th><th>Devolución</th><th>Total</th><th>Estado</th><th>Acciones</th>
            </tr></thead>
            <tbody>
              {pagination.paginatedItems.map((r) => (
                <tr key={r.idReserva || r.codigoReserva}>
                  <td><code>{r.codigoReserva}</code></td>
                  <td>{r.nombreCliente || r.cliente}</td>
                  <td>{r.vehiculo || r.descripcionVehiculo || `${r.placaVehiculo || ''}`}</td>
                  <td>{r.fechaHoraRecogida ? new Date(r.fechaHoraRecogida).toLocaleDateString() : '-'}</td>
                  <td>{r.fechaHoraDevolucion ? new Date(r.fechaHoraDevolucion).toLocaleDateString() : '-'}</td>
                  <td><strong>${Number(r.totalReserva || r.total || 0).toFixed(2)}</strong></td>
                  <td><span className={`status-badge status-badge--${r.estadoReserva === 'CONFIRMADA' ? 'success' : r.estadoReserva === 'PENDIENTE' ? 'warning' : 'danger'}`}>{r.estadoReserva}</span></td>
                  <td className="table-actions">
                    {r.estadoReserva === 'PENDIENTE' && (
                      <button className="icon-btn" onClick={() => openEdit(r)} title="Editar">
                        <Pencil size={15} />
                      </button>
                    )}
                    {r.estadoReserva === 'PENDIENTE' && <button className="icon-btn icon-btn--success" onClick={() => confirmar(r.idReserva)} title="Confirmar"><CheckCircle size={15} /></button>}
                    {(r.estadoReserva === 'PENDIENTE' || r.estadoReserva === 'CONFIRMADA') && <button className="icon-btn icon-btn--danger" onClick={() => cancelar(r.idReserva)} title="Cancelar"><XCircle size={15} /></button>}
                  </td>
                </tr>
              ))}
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
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal modal--lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal__header"><h2>{editingReserva ? 'Editar Reserva' : 'Nueva Reserva'}</h2><button className="icon-btn" onClick={() => { setShowModal(false); setEditingReserva(null); }}><X size={18} /></button></div>
            <form onSubmit={handleSave} className="modal__body">
              <div className="form-row">
                <div className="form-group"><label className="form-label">Cliente</label>
                  <select className="form-input" required disabled={!!editingReserva} value={form.idCliente} onChange={(e) => setForm({...form, idCliente: e.target.value})}>
                    <option value="">Seleccionar cliente</option>
                    {clientes.map(c => <option key={c.idCliente} value={c.idCliente}>{c.nombreCompleto} - {c.numeroIdentificacion}</option>)}
                  </select></div>
                <div className="form-group"><label className="form-label">Vehículo</label>
                  <select className="form-input" required value={form.idVehiculo} onChange={(e) => setForm({...form, idVehiculo: e.target.value})}>
                    <option value="">Seleccionar vehículo</option>
                    {vehiculos.map(v => <option key={v.idVehiculo} value={v.idVehiculo}>{v.placa || v.marca} - {v.marca} {v.modelo} (${Number(v.precioBaseDia).toFixed(2)}/día)</option>)}
                  </select></div>
              </div>
              <div className="form-row">
                <div className="form-group"><label className="form-label">Sucursal Recogida</label>
                  <select className="form-input" required value={form.idLocalizacionRecogida} onChange={(e) => setForm({...form, idLocalizacionRecogida: e.target.value})}>
                    <option value="">Seleccionar</option>
                    {localizaciones.map(l => <option key={l.idLocalizacion || l.id} value={l.idLocalizacion || l.id}>{l.nombreLocalizacion || l.nombre}</option>)}
                  </select></div>
                <div className="form-group"><label className="form-label">Sucursal Devolución</label>
                  <select className="form-input" required value={form.idLocalizacionDevolucion} onChange={(e) => setForm({...form, idLocalizacionDevolucion: e.target.value})}>
                    <option value="">Seleccionar</option>
                    {localizaciones.map(l => <option key={l.idLocalizacion || l.id} value={l.idLocalizacion || l.id}>{l.nombreLocalizacion || l.nombre}</option>)}
                  </select></div>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <DateTimePicker
                    id="reserva-fecha-recogida"
                    label="Fecha y hora de Recogida *"
                    value={form.fechaHoraRecogida}
                    onChange={(val) => setForm({ ...form, fechaHoraRecogida: val })}
                  />
                </div>
                <div className="form-group">
                  <DateTimePicker
                    id="reserva-fecha-devolucion"
                    label="Fecha y hora de Devolución *"
                    value={form.fechaHoraDevolucion}
                    minDate={form.fechaHoraRecogida}
                    onChange={(val) => setForm({ ...form, fechaHoraDevolucion: val })}
                  />
                </div>
              </div>
              <div className="form-group">
                <label className="form-label">Extras</label>
                {form.extras.map((ex, i) => {
                  const condId = conductorExtra?.idExtra;
                  const isCondAdicRow = condId != null && String(ex.idExtra) === String(condId);

                  return (
                    <div key={i} className="form-row" style={{ marginBottom: '0.5rem' }}>
                    <select
                      className="form-input"
                      value={ex.idExtra}
                      disabled={isCondAdicRow}
                      onChange={(e) => {
                        if (isCondAdicRow) return;
                        const n = [...form.extras];
                        n[i].idExtra = e.target.value;
                        setForm({ ...form, extras: n });
                      }}
                    >
                      <option value="">Seleccionar extra</option>
                      {extras.map((ext) => {
                        const isCond = isConductorExtra(ext);
                        const lockOption = isCond && conductoresAdicionalesValidos === 0;
                        return (
                          <option key={ext.idExtra ?? ext.id} value={ext.idExtra} disabled={lockOption}>
                            {ext.nombreExtra || ext.nombre}
                            {' '}
                            (${Number(ext.valorFijo || 0).toFixed(2)}/día)
                            {lockOption ? ' (requiere conductor adicional)' : ''}
                          </option>
                        );
                      })}
                    </select>
                    <input
                      type="number"
                      min="1"
                      className="form-input"
                      style={{ maxWidth: 80 }}
                      value={isCondAdicRow ? conductoresAdicionalesValidos : ex.cantidad}
                      disabled={isCondAdicRow}
                      onChange={(e) => {
                        const n = [...form.extras];
                        n[i].cantidad = Number(e.target.value);
                        setForm({ ...form, extras: n });
                      }}
                    />
                    <button
                      type="button"
                      className="icon-btn icon-btn--danger"
                      disabled={isCondAdicRow}
                      onClick={() => removeExtra(i)}
                      title={isCondAdicRow ? 'Este extra se maneja automáticamente' : 'Eliminar extra'}
                    >
                      <X size={14} />
                    </button>
                  </div>
                  );
                })}
                <button type="button" className="btn btn--ghost btn--sm" onClick={addExtra}><Plus size={14} /> Agregar extra</button>
              </div>
              {!editingReserva && (
                <div className="form-group">
                  <label className="form-label">Conductores adicionales</label>
                  {conductores.map((c, i) => (
                    <div key={i} className="form-row" style={{ marginBottom: '0.5rem' }}>
                      <select className="form-input" value={c.tipoIdentificacion} onChange={(e) => { const n = [...conductores]; n[i].tipoIdentificacion = e.target.value; setConductores(n); }} style={{ maxWidth: 120 }}>
                        <option value="CED">CED</option>
                        <option value="PAS">PAS</option>
                      </select>
                      <input className="form-input" placeholder="Identificación" value={c.numeroIdentificacion} onChange={(e) => { const n = [...conductores]; n[i].numeroIdentificacion = e.target.value; setConductores(n); }} />
                      <input className="form-input" placeholder="Nombre" value={c.nombre1} onChange={(e) => { const n = [...conductores]; n[i].nombre1 = e.target.value; setConductores(n); }} />
                      <input className="form-input" placeholder="Apellido" value={c.apellido1} onChange={(e) => { const n = [...conductores]; n[i].apellido1 = e.target.value; setConductores(n); }} />
                      <input className="form-input" placeholder="Identificación" value={c.numeroLicencia} onChange={(e) => { const n = [...conductores]; n[i].numeroLicencia = e.target.value; setConductores(n); }} />
                      <input className="form-input" type="number" min="18" placeholder="Edad" value={c.edadConductor} onChange={(e) => { const n = [...conductores]; n[i].edadConductor = e.target.value; setConductores(n); }} style={{ maxWidth: 90 }} />
                      <button type="button" className="icon-btn icon-btn--danger" onClick={() => removeConductor(i)}><X size={14} /></button>
                    </div>
                  ))}
                  <button type="button" className="btn btn--ghost btn--sm" onClick={addConductor}>
                    <Plus size={14} /> Agregar conductor adicional
                  </button>
                </div>
              )}
              <div className="modal__footer">
                <button type="button" className="btn btn--ghost" onClick={() => { setShowModal(false); setEditingReserva(null); }}>Cancelar</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>
                  {saving ? <><Loader2 size={16} className="spin" /> Guardando...</> : (editingReserva ? 'Guardar cambios' : 'Crear Reserva')}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
