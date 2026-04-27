import { useState, useEffect } from 'react';
import { pagosApi } from '../../api/pagosApi';
import { toast } from 'sonner';
import { CreditCard, Search, Plus, X, Loader2, RefreshCw } from 'lucide-react';

export default function PagosPage() {
  const [pagos, setPagos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({
    idReserva: '', montoPago: '', metodoPago: 'TARJETA_CREDITO', referenciaPago: '', observaciones: '',
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

  const handleCreate = async (e) => {
    e.preventDefault(); setSaving(true);
    try {
      await pagosApi.create({ ...form, idReserva: Number(form.idReserva), montoPago: Number(form.montoPago) });
      toast.success('Pago registrado');
      setShowModal(false);
      loadPagos();
    } catch (e) { toast.error(e.response?.data?.message || 'Error al registrar pago'); }
    finally { setSaving(false); }
  };

  const filtered = pagos.filter(p => {
    const text = `${p.codigoPago || ''} ${p.codigoReserva || ''} ${p.nombreCliente || ''} ${p.metodoPago || ''}`.toLowerCase();
    return text.includes(search.toLowerCase());
  });

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div><h1><CreditCard size={24} /> Pagos</h1><p>{pagos.length} pagos registrados</p></div>
        <div className="module-page__actions">
          <button className="btn btn--outline btn--sm" onClick={loadPagos} disabled={loading}>
            <RefreshCw size={16} className={loading ? 'spin' : ''} /> Recargar
          </button>
          <button className="btn btn--primary" onClick={() => setShowModal(true)}><Plus size={16} /> Registrar Pago</button>
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
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead><tr><th>Código</th><th>Reserva</th><th>Cliente</th><th>Monto</th><th>Método</th><th>Referencia</th><th>Fecha</th><th>Estado</th></tr></thead>
            <tbody>
              {filtered.map(p => (
                <tr key={p.idPago}>
                  <td><code>{p.codigoPago}</code></td>
                  <td><code>{p.codigoReserva || '-'}</code></td>
                  <td>{p.nombreCliente || '-'}</td>
                  <td><strong>${Number(p.monto || 0).toFixed(2)}</strong></td>
                  <td><span className="badge badge--outline">{p.metodoPago}</span></td>
                  <td>{p.referenciaExterna || '-'}</td>
                  <td>{p.fechaPagoUtc ? new Date(p.fechaPagoUtc).toLocaleDateString() : '-'}</td>
                  <td><span className={`status-badge status-badge--${p.estadoPago === 'COMPLETADO' ? 'success' : 'warning'}`}>{p.estadoPago}</span></td>
                </tr>
              ))}
              {filtered.length === 0 && <tr><td colSpan={8} className="table-empty">No hay pagos</td></tr>}
            </tbody>
          </table>
        </div>
      )}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal__header"><h2>Registrar Pago</h2><button className="icon-btn" onClick={() => setShowModal(false)}><X size={18} /></button></div>
            <form onSubmit={handleCreate} className="modal__body">
              <div className="form-row">
                <div className="form-group"><label className="form-label">ID Reserva</label>
                  <input type="number" className="form-input" required value={form.idReserva} onChange={e => setForm({...form, idReserva: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Monto ($)</label>
                  <input type="number" step="0.01" className="form-input" required value={form.montoPago} onChange={e => setForm({...form, montoPago: e.target.value})} /></div>
              </div>
              <div className="form-row">
                <div className="form-group"><label className="form-label">Método de Pago</label>
                  <select className="form-input" value={form.metodoPago} onChange={e => setForm({...form, metodoPago: e.target.value})}>
                    <option value="TARJETA_CREDITO">Tarjeta Crédito</option><option value="TARJETA_DEBITO">Tarjeta Débito</option>
                    <option value="EFECTIVO">Efectivo</option><option value="TRANSFERENCIA">Transferencia</option>
                  </select></div>
                <div className="form-group"><label className="form-label">Referencia</label>
                  <input className="form-input" value={form.referenciaPago} onChange={e => setForm({...form, referenciaPago: e.target.value})} placeholder="# transacción" /></div>
              </div>
              <div className="form-group"><label className="form-label">Observaciones</label>
                <input className="form-input" value={form.observaciones} onChange={e => setForm({...form, observaciones: e.target.value})} /></div>
              <div className="modal__footer">
                <button type="button" className="btn btn--ghost" onClick={() => setShowModal(false)}>Cancelar</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>{saving ? 'Registrando...' : 'Registrar Pago'}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
