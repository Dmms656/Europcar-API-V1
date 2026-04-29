import { useState, useEffect } from 'react';
import { pagosApi } from '../../api/pagosApi';
import { toast } from 'sonner';
import { CreditCard, Search, Plus, X, Loader2, RefreshCw, Pencil } from 'lucide-react';
import { useClientPagination } from '../../hooks/useClientPagination';
import PaginationControls from '../../components/ui/PaginationControls';

export default function PagosPage() {
  const [pagos, setPagos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [editingPago, setEditingPago] = useState(null);
  const [form, setForm] = useState({
    reservaRef: '',
    idContrato: '',
    idCliente: '',
    tipoPago: 'COBRO',
    metodoPago: 'TARJETA',
    estadoPago: 'APROBADO',
    monto: '',
    referenciaExterna: '',
    observaciones: '',
  });

  useEffect(() => { loadPagos(); }, []);

  const loadPagos = async () => {
    setLoading(true);
    try {
      const res = await pagosApi.getAll();
      setPagos(res.data?.data || []);
    } catch (e) { toast.error('Error al cargar pagos'); }
    finally { setLoading(false); }
  };

  const resetForm = () => {
    setEditingPago(null);
    setForm({
      reservaRef: '',
      idContrato: '',
      idCliente: '',
      tipoPago: 'COBRO',
      metodoPago: 'TARJETA',
      estadoPago: 'APROBADO',
      monto: '',
      referenciaExterna: '',
      observaciones: '',
    });
  };

  const openCreate = () => {
    resetForm();
    setShowModal(true);
  };

  const openEdit = (p) => {
    setEditingPago(p);
    setForm({
      reservaRef: p.codigoReserva || (p.idReserva ? String(p.idReserva) : ''),
      idContrato: p.idContrato ? String(p.idContrato) : '',
      idCliente: p.idCliente ? String(p.idCliente) : '',
      tipoPago: p.tipoPago || 'COBRO',
      metodoPago: p.metodoPago || 'TARJETA',
      estadoPago: p.estadoPago || 'APROBADO',
      monto: p.monto != null ? String(p.monto) : '',
      referenciaExterna: p.referenciaExterna || '',
      observaciones: p.observacionesPago || '',
    });
    setShowModal(true);
  };

  const buildPayload = () => {
    const reservaRef = form.reservaRef.trim();
    const payload = {
      idContrato: form.idContrato ? Number(form.idContrato) : null,
      idCliente: Number(form.idCliente),
      tipoPago: form.tipoPago,
      metodoPago: form.metodoPago,
      estadoPago: form.estadoPago,
      monto: Number(form.monto),
      referenciaExterna: form.referenciaExterna.trim() || null,
      observaciones: form.observaciones.trim() || null,
    };

    if (/^\d+$/.test(reservaRef)) {
      payload.idReserva = Number(reservaRef);
      payload.codigoReserva = null;
    } else {
      payload.idReserva = null;
      payload.codigoReserva = reservaRef || null;
    }

    return payload;
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = buildPayload();
      if (!payload.idReserva && !payload.codigoReserva && !payload.idContrato) {
        throw new Error('Debes indicar reserva (ID o código) o contrato.');
      }
      if (!payload.idCliente) {
        throw new Error('El ID de cliente es requerido.');
      }
      if (!(payload.monto > 0)) {
        throw new Error('El monto debe ser mayor a cero.');
      }

      if (editingPago) {
        await pagosApi.update(editingPago.idPago, payload);
        toast.success('Pago actualizado');
      } else {
        await pagosApi.create(payload);
        toast.success('Pago registrado');
      }

      setShowModal(false);
      resetForm();
      loadPagos();
    } catch (e) { toast.error(e.response?.data?.message || e.message || 'Error al guardar pago'); }
    finally { setSaving(false); }
  };

  const filtered = pagos.filter(p => {
    const text = `${p.codigoPago || ''} ${p.codigoReserva || ''} ${p.nombreCliente || ''} ${p.metodoPago || ''}`.toLowerCase();
    return text.includes(search.toLowerCase());
  });
  const pagination = useClientPagination(filtered, 10);

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div><h1><CreditCard size={24} /> Pagos</h1><p>{pagos.length} pagos registrados</p></div>
        <div className="module-page__actions">
          <button className="btn btn--outline btn--sm" onClick={loadPagos} disabled={loading}>
            <RefreshCw size={16} className={loading ? 'spin' : ''} /> Recargar
          </button>
          <button className="btn btn--primary" onClick={openCreate}><Plus size={16} /> Registrar Pago</button>
        </div>
      </div>
      <div className="module-page__toolbar">
        <div className="search-box"><Search size={16} />
          <input placeholder="Buscar por código, reserva, cliente o método..." value={search}
            onChange={(e) => setSearch(e.target.value)} />
        </div>
      </div>
      {loading ? (
        <div className="module-loading"><Loader2 size={24} className="spin" /> Cargando pagos...</div>
      ) : (
        <>
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead><tr><th>Código</th><th>Reserva</th><th>Cliente</th><th>Monto</th><th>Método</th><th>Referencia</th><th>Fecha</th><th>Estado</th><th>Acciones</th></tr></thead>
            <tbody>
              {pagination.paginatedItems.map(p => (
                <tr key={p.idPago}>
                  <td><code>{p.codigoPago}</code></td>
                  <td><code>{p.codigoReserva || '-'}</code></td>
                  <td>{p.nombreCliente || '-'}</td>
                  <td><strong>${Number(p.monto || 0).toFixed(2)}</strong></td>
                  <td><span className="badge badge--outline">{p.metodoPago}</span></td>
                  <td>{p.referenciaExterna || '-'}</td>
                  <td>{p.fechaPagoUtc ? new Date(p.fechaPagoUtc).toLocaleDateString() : '-'}</td>
                  <td><span className={`status-badge status-badge--${p.estadoPago === 'COMPLETADO' ? 'success' : 'warning'}`}>{p.estadoPago}</span></td>
                  <td className="table-actions">
                    <button className="icon-btn" onClick={() => openEdit(p)} title="Editar pago">
                      <Pencil size={15} />
                    </button>
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && <tr><td colSpan={9} className="table-empty">No hay pagos</td></tr>}
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
        <div className="modal-overlay" onClick={() => { setShowModal(false); resetForm(); }}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal__header"><h2>{editingPago ? 'Editar Pago' : 'Registrar Pago'}</h2><button className="icon-btn" onClick={() => { setShowModal(false); resetForm(); }}><X size={18} /></button></div>
            <form onSubmit={handleSave} className="modal__body">
              <div className="form-row">
                <div className="form-group"><label className="form-label">Reserva (ID o Código)</label>
                  <input className="form-input" value={form.reservaRef} onChange={e => setForm({...form, reservaRef: e.target.value})} placeholder="Ej: 123 o RSV-ABC123" /></div>
                <div className="form-group"><label className="form-label">ID Contrato (opcional)</label>
                  <input type="number" className="form-input" value={form.idContrato} onChange={e => setForm({...form, idContrato: e.target.value})} /></div>
              </div>
              <div className="form-row">
                <div className="form-group"><label className="form-label">ID Cliente</label>
                  <input type="number" className="form-input" required value={form.idCliente} onChange={e => setForm({...form, idCliente: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Monto ($)</label>
                  <input type="number" step="0.01" className="form-input" required value={form.monto} onChange={e => setForm({...form, monto: e.target.value})} /></div>
              </div>
              <div className="form-row">
                <div className="form-group"><label className="form-label">Tipo de Pago</label>
                  <select className="form-input" value={form.tipoPago} onChange={e => setForm({...form, tipoPago: e.target.value})}>
                    <option value="COBRO">Cobro</option><option value="REEMBOLSO">Reembolso</option>
                  </select></div>
                <div className="form-group"><label className="form-label">Método de Pago</label>
                  <select className="form-input" value={form.metodoPago} onChange={e => setForm({...form, metodoPago: e.target.value})}>
                    <option value="TARJETA">Tarjeta</option><option value="EFECTIVO">Efectivo</option>
                    <option value="TRANSFERENCIA">Transferencia</option><option value="PAYPAL">PayPal</option>
                  </select></div>
              </div>
              <div className="form-row">
                <div className="form-group"><label className="form-label">Estado</label>
                  <select className="form-input" value={form.estadoPago} onChange={e => setForm({...form, estadoPago: e.target.value})}>
                    <option value="APROBADO">Aprobado</option><option value="PENDIENTE">Pendiente</option>
                    <option value="ANULADO">Anulado</option><option value="REEMBOLSADO">Reembolsado</option>
                  </select></div>
                <div className="form-group"><label className="form-label">Referencia</label>
                  <input className="form-input" value={form.referenciaExterna} onChange={e => setForm({...form, referenciaExterna: e.target.value})} placeholder="# transacción" /></div>
              </div>
              <div className="form-group"><label className="form-label">Observaciones</label>
                <input className="form-input" value={form.observaciones} onChange={e => setForm({...form, observaciones: e.target.value})} /></div>
              <div className="modal__footer">
                <button type="button" className="btn btn--ghost" onClick={() => { setShowModal(false); resetForm(); }}>Cancelar</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>{saving ? 'Guardando...' : (editingPago ? 'Guardar cambios' : 'Registrar Pago')}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
