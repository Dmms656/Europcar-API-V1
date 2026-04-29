import { useState, useEffect } from 'react';
import { reservasApi } from '../../api/reservasApi';
import { vehiculosApi } from '../../api/vehiculosApi';
import { clientesApi } from '../../api/clientesApi';
import { catalogosApi } from '../../api/catalogosApi';
import { toast } from 'sonner';
import { Plus, Search, CheckCircle, XCircle, Loader2, CalendarCheck, X, RefreshCw, Pencil } from 'lucide-react';
import { useClientPagination } from '../../hooks/useClientPagination';
import PaginationControls from '../../components/ui/PaginationControls';

export default function ReservasPage() {
  const [reservas, setReservas] = useState([]);
  const [clientes, setClientes] = useState([]);
  const [vehiculos, setVehiculos] = useState([]);
  const [localizaciones, setLocalizaciones] = useState([]);
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
  const pagination = useClientPagination(reservas, 10);

  useEffect(() => { loadAll(); }, []);

  const loadAll = async () => {
    setLoading(true);
    try {
      const [cRes, vRes, lRes, eRes] = await Promise.all([
        clientesApi.getAll(), vehiculosApi.getDisponibles({}),
        catalogosApi.getLocalizaciones(), catalogosApi.getExtras(),
      ]);
      const clientesList = cRes.data?.data || [];
      setClientes(clientesList);
      setVehiculos(vRes.data?.data || []);
      setLocalizaciones(lRes.data?.data || []);
      setExtras(eRes.data?.data || []);

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
      const payload = {
        ...form, idCliente: Number(form.idCliente), idVehiculo: Number(form.idVehiculo),
        idLocalizacionRecogida: Number(form.idLocalizacionRecogida),
        idLocalizacionDevolucion: Number(form.idLocalizacionDevolucion),
        extras: form.extras.filter(ex => ex.idExtra && ex.cantidad > 0).map(ex => ({ idExtra: Number(ex.idExtra), cantidad: Number(ex.cantidad) })),
      };
      const res = await reservasApi.create(payload);
      toast.success(`Reserva creada: ${res.data?.data?.codigoReserva || 'OK'}`);
      setShowModal(false);
      loadAll();
    } catch (e) { toast.error(e.response?.data?.message || 'Error al crear reserva'); }
    finally { setSaving(false); }
  };

  const openEdit = (r) => {
    setEditingReserva(r);
    setForm({
      ...form,
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
      const payload = {
        idVehiculo: Number(form.idVehiculo),
        idLocalizacionRecogida: Number(form.idLocalizacionRecogida),
        idLocalizacionDevolucion: Number(form.idLocalizacionDevolucion),
        fechaHoraRecogida: form.fechaHoraRecogida,
        fechaHoraDevolucion: form.fechaHoraDevolucion,
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

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div><h1><CalendarCheck size={24} /> Reservas</h1><p>{reservas.length} reservas encontradas</p></div>
        <div className="module-page__actions">
          <button className="btn btn--outline btn--sm" onClick={loadAll}><RefreshCw size={16} /> Recargar</button>
          <button className="btn btn--primary" onClick={() => setShowModal(true)}><Plus size={16} /> Nueva Reserva</button>
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
                    {(r.estadoReserva === 'PENDIENTE' || r.estadoReserva === 'CONFIRMADA') && (
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
                  <select className="form-input" required value={form.idCliente} onChange={(e) => setForm({...form, idCliente: e.target.value})}>
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
                <div className="form-group"><label className="form-label">Fecha/Hora Recogida</label>
                  <input type="datetime-local" className="form-input" required value={form.fechaHoraRecogida} onChange={(e) => setForm({...form, fechaHoraRecogida: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Fecha/Hora Devolución</label>
                  <input type="datetime-local" className="form-input" required value={form.fechaHoraDevolucion} onChange={(e) => setForm({...form, fechaHoraDevolucion: e.target.value})} /></div>
              </div>
              <div className="form-group">
                <label className="form-label">Extras</label>
                {form.extras.map((ex, i) => (
                  <div key={i} className="form-row" style={{marginBottom:'0.5rem'}}>
                    <select className="form-input" value={ex.idExtra} onChange={(e) => { const n = [...form.extras]; n[i].idExtra = e.target.value; setForm({...form, extras: n}); }}>
                      <option value="">Seleccionar extra</option>
                      {extras.map(ext => <option key={ext.idExtra || ext.id} value={ext.idExtra || ext.id}>{ext.nombreExtra || ext.nombre} (${Number(ext.precioDiario || ext.precio || 0).toFixed(2)}/día)</option>)}
                    </select>
                    <input type="number" min="1" className="form-input" style={{maxWidth:80}} value={ex.cantidad} onChange={(e) => { const n = [...form.extras]; n[i].cantidad = Number(e.target.value); setForm({...form, extras: n}); }} />
                    <button type="button" className="icon-btn icon-btn--danger" onClick={() => removeExtra(i)}><X size={14} /></button>
                  </div>
                ))}
                <button type="button" className="btn btn--ghost btn--sm" onClick={addExtra}><Plus size={14} /> Agregar extra</button>
              </div>
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
