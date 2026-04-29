import { useState, useEffect } from 'react';
import { contratosApi } from '../../api/contratosApi';
import { toast } from 'sonner';
import { Search, Loader2, FileText, ArrowRightCircle, ArrowLeftCircle, X, Plus, Pencil } from 'lucide-react';
import { useClientPagination } from '../../hooks/useClientPagination';
import PaginationControls from '../../components/ui/PaginationControls';

export default function ContratosPage() {
  const [contratos, setContratos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showCheckout, setShowCheckout] = useState(false);
  const [showCheckin, setShowCheckin] = useState(false);
  const [showCrear, setShowCrear] = useState(false);
  const [showEdit, setShowEdit] = useState(false);
  const [saving, setSaving] = useState(false);
  const [crearForm, setCrearForm] = useState({ reservaRef: '' });
  const [checkoutForm, setCheckoutForm] = useState({ idContrato: '', kilometrajeSalida: '', nivelCombustibleSalida: '', observacionesSalida: '' });
  const [checkinForm, setCheckinForm] = useState({ idContrato: '', kilometrajeEntrada: '', nivelCombustibleEntrada: '', observacionesEntrada: '', cargosAdicionales: 0 });
  const [editForm, setEditForm] = useState({
    idContrato: '', fechaHoraSalida: '', fechaHoraPrevistaDevolucion: '', kilometrajeSalida: '', nivelCombustibleSalida: '', estadoContrato: 'ABIERTO', observaciones: '',
  });
  const pagination = useClientPagination(contratos, 10);

  useEffect(() => { loadContratos(); }, []);

  const loadContratos = async () => {
    setLoading(true);
    try { const res = await contratosApi.getAll(); setContratos(res.data?.data || []); }
    catch (e) { toast.error('Error al cargar contratos'); }
    finally { setLoading(false); }
  };

  const crearContrato = async (e) => {
    e.preventDefault(); setSaving(true);
    try {
      const ref = (crearForm.reservaRef || '').trim();
      if (!ref) throw new Error('Ingresa el ID o el código de la reserva.');

      const isNumeric = /^\d+$/.test(ref);
      const payload = isNumeric
        ? { idReserva: Number(ref) }
        : { codigoReserva: ref.toUpperCase() };

      await contratosApi.create(payload);
      toast.success('Contrato creado');
      setShowCrear(false);
      setCrearForm({ reservaRef: '' });
      loadContratos();
    } catch (e) { toast.error(e.response?.data?.message || e.message || 'Error'); }
    finally { setSaving(false); }
  };

  const doCheckout = async (e) => {
    e.preventDefault(); setSaving(true);
    try {
      await contratosApi.checkout({
        idContrato: Number(checkoutForm.idContrato),
        kilometraje: Number(checkoutForm.kilometrajeSalida),
        nivelCombustible: Number(checkoutForm.nivelCombustibleSalida),
        Limpio: true,
        observaciones: checkoutForm.observacionesSalida || null,
      });
      toast.success('Check-out registrado'); setShowCheckout(false); loadContratos();
    } catch (e) { toast.error(e.response?.data?.message || 'Error'); }
    finally { setSaving(false); }
  };

  const doCheckin = async (e) => {
    e.preventDefault(); setSaving(true);
    try {
      await contratosApi.checkin({
        idContrato: Number(checkinForm.idContrato),
        kilometraje: Number(checkinForm.kilometrajeEntrada),
        nivelCombustible: Number(checkinForm.nivelCombustibleEntrada),
        Limpio: true,
        observaciones: checkinForm.observacionesEntrada || null,
      });
      toast.success('Check-in registrado'); setShowCheckin(false); loadContratos();
    } catch (e) { toast.error(e.response?.data?.message || 'Error'); }
    finally { setSaving(false); }
  };

  const openEdit = (c) => {
    setEditForm({
      idContrato: String(c.idContrato),
      fechaHoraSalida: c.fechaHoraSalida ? new Date(c.fechaHoraSalida).toISOString().slice(0, 16) : '',
      fechaHoraPrevistaDevolucion: c.fechaHoraPrevistaDevolucion ? new Date(c.fechaHoraPrevistaDevolucion).toISOString().slice(0, 16) : '',
      kilometrajeSalida: String(c.kilometrajeSalida || 0),
      nivelCombustibleSalida: String(c.nivelCombustibleSalida || 0),
      estadoContrato: c.estadoContrato || 'ABIERTO',
      observaciones: c.observacionesContrato || '',
    });
    setShowEdit(true);
  };

  const doEdit = async (e) => {
    e.preventDefault(); setSaving(true);
    try {
      await contratosApi.update(Number(editForm.idContrato), {
        fechaHoraSalida: editForm.fechaHoraSalida,
        fechaHoraPrevistaDevolucion: editForm.fechaHoraPrevistaDevolucion,
        kilometrajeSalida: Number(editForm.kilometrajeSalida),
        nivelCombustibleSalida: Number(editForm.nivelCombustibleSalida),
        estadoContrato: editForm.estadoContrato,
        observaciones: editForm.observaciones || null,
      });
      toast.success('Contrato actualizado');
      setShowEdit(false);
      loadContratos();
    } catch (e) { toast.error(e.response?.data?.message || 'Error'); }
    finally { setSaving(false); }
  };

  return (
    <div className="module-page">
      <div className="module-page__header">
        <div><h1><FileText size={24} /> Contratos</h1><p>{contratos.length} contratos</p></div>
        <div className="module-page__actions">
          <button className="btn btn--primary" onClick={() => setShowCrear(true)}><Plus size={16} /> Crear Contrato</button>
          <button className="btn btn--outline" onClick={() => setShowCheckout(true)}><ArrowRightCircle size={16} /> Check-Out</button>
          <button className="btn btn--outline" onClick={() => setShowCheckin(true)}><ArrowLeftCircle size={16} /> Check-In</button>
        </div>
      </div>
      {loading ? (
        <div className="module-loading"><Loader2 size={24} className="spin" /> Cargando...</div>
      ) : (
        <>
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead><tr><th>ID</th><th>Código Reserva</th><th>Cliente</th><th>Vehículo</th><th>Inicio</th><th>Fin</th><th>Estado</th><th>Acciones</th></tr></thead>
            <tbody>
              {pagination.paginatedItems.map(c => (
                <tr key={c.idContrato}>
                  <td>{c.idContrato}</td>
                  <td><code>{c.codigoReserva || c.reservaCodigo || '-'}</code></td>
                  <td>{c.nombreCliente || c.cliente || '-'}</td>
                  <td>{c.vehiculo || c.descripcionVehiculo || '-'}</td>
                  <td>{c.fechaHoraSalida ? new Date(c.fechaHoraSalida).toLocaleDateString() : '-'}</td>
                  <td>{c.fechaHoraPrevistaDevolucion ? new Date(c.fechaHoraPrevistaDevolucion).toLocaleDateString() : '-'}</td>
                  <td><span className={`status-badge status-badge--${c.estadoContrato === 'CERRADO' ? 'success' : c.estadoContrato === 'ABIERTO' ? 'warning' : 'danger'}`}>{c.estadoContrato}</span></td>
                  <td className="table-actions">
                    {c.estadoContrato !== 'CERRADO' && (
                      <button className="icon-btn" onClick={() => openEdit(c)} title="Editar">
                        <Pencil size={15} />
                      </button>
                    )}
                  </td>
                </tr>
              ))}
              {contratos.length === 0 && <tr><td colSpan={8} className="table-empty">No hay contratos</td></tr>}
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
      {/* Crear Contrato Modal */}
      {showCrear && (
        <div className="modal-overlay" onClick={() => setShowCrear(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal__header"><h2>Crear Contrato</h2><button className="icon-btn" onClick={() => setShowCrear(false)}><X size={18} /></button></div>
            <form onSubmit={crearContrato} className="modal__body">
              <div className="form-group">
                <label className="form-label">ID o Código de Reserva (confirmada)</label>
                <input
                  type="text"
                  className="form-input"
                  required
                  value={crearForm.reservaRef}
                  onChange={e => setCrearForm({ ...crearForm, reservaRef: e.target.value })}
                  placeholder="Ej: 1 o RES-0002"
                />
                <small className="form-help">Acepta el ID numérico o el código de reserva (ej: RES-0002).</small>
              </div>
              <div className="modal__footer">
                <button type="button" className="btn btn--ghost" onClick={() => setShowCrear(false)}>Cancelar</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>{saving ? 'Creando...' : 'Crear'}</button>
              </div>
            </form>
          </div>
        </div>
      )}
      {/* Checkout Modal */}
      {showCheckout && (
        <div className="modal-overlay" onClick={() => setShowCheckout(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal__header"><h2>Registrar Check-Out</h2><button className="icon-btn" onClick={() => setShowCheckout(false)}><X size={18} /></button></div>
            <form onSubmit={doCheckout} className="modal__body">
              <div className="form-group"><label className="form-label">ID Contrato</label>
                <input type="number" className="form-input" required value={checkoutForm.idContrato} onChange={e => setCheckoutForm({...checkoutForm, idContrato: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Kilometraje de Salida</label>
                <input type="number" className="form-input" required value={checkoutForm.kilometrajeSalida} onChange={e => setCheckoutForm({...checkoutForm, kilometrajeSalida: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Nivel Combustible Salida</label>
                <input className="form-input" value={checkoutForm.nivelCombustibleSalida} onChange={e => setCheckoutForm({...checkoutForm, nivelCombustibleSalida: e.target.value})} placeholder="Ej: LLENO, 3/4, 1/2" /></div>
              <div className="form-group"><label className="form-label">Observaciones</label>
                <input className="form-input" value={checkoutForm.observacionesSalida} onChange={e => setCheckoutForm({...checkoutForm, observacionesSalida: e.target.value})} /></div>
              <div className="modal__footer">
                <button type="button" className="btn btn--ghost" onClick={() => setShowCheckout(false)}>Cancelar</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>{saving ? 'Registrando...' : 'Registrar Check-Out'}</button>
              </div>
            </form>
          </div>
        </div>
      )}
      {/* Checkin Modal */}
      {showCheckin && (
        <div className="modal-overlay" onClick={() => setShowCheckin(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal__header"><h2>Registrar Check-In (Devolución)</h2><button className="icon-btn" onClick={() => setShowCheckin(false)}><X size={18} /></button></div>
            <form onSubmit={doCheckin} className="modal__body">
              <div className="form-group"><label className="form-label">ID Contrato</label>
                <input type="number" className="form-input" required value={checkinForm.idContrato} onChange={e => setCheckinForm({...checkinForm, idContrato: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Kilometraje de Entrada</label>
                <input type="number" className="form-input" required value={checkinForm.kilometrajeEntrada} onChange={e => setCheckinForm({...checkinForm, kilometrajeEntrada: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Nivel Combustible Entrada</label>
                <input className="form-input" value={checkinForm.nivelCombustibleEntrada} onChange={e => setCheckinForm({...checkinForm, nivelCombustibleEntrada: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Cargos Adicionales ($)</label>
                <input type="number" step="0.01" className="form-input" value={checkinForm.cargosAdicionales} onChange={e => setCheckinForm({...checkinForm, cargosAdicionales: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Observaciones</label>
                <input className="form-input" value={checkinForm.observacionesEntrada} onChange={e => setCheckinForm({...checkinForm, observacionesEntrada: e.target.value})} /></div>
              <div className="modal__footer">
                <button type="button" className="btn btn--ghost" onClick={() => setShowCheckin(false)}>Cancelar</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>{saving ? 'Registrando...' : 'Registrar Check-In'}</button>
              </div>
            </form>
          </div>
        </div>
      )}
      {/* Editar Contrato Modal */}
      {showEdit && (
        <div className="modal-overlay" onClick={() => setShowEdit(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal__header"><h2>Editar Contrato</h2><button className="icon-btn" onClick={() => setShowEdit(false)}><X size={18} /></button></div>
            <form onSubmit={doEdit} className="modal__body">
              <div className="form-row">
                <div className="form-group"><label className="form-label">Fecha salida</label>
                  <input type="datetime-local" className="form-input" required value={editForm.fechaHoraSalida} onChange={e => setEditForm({...editForm, fechaHoraSalida: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Fecha devolución prevista</label>
                  <input type="datetime-local" className="form-input" required value={editForm.fechaHoraPrevistaDevolucion} onChange={e => setEditForm({...editForm, fechaHoraPrevistaDevolucion: e.target.value})} /></div>
              </div>
              <div className="form-row">
                <div className="form-group"><label className="form-label">Kilometraje salida</label>
                  <input type="number" className="form-input" required value={editForm.kilometrajeSalida} onChange={e => setEditForm({...editForm, kilometrajeSalida: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Nivel combustible salida</label>
                  <input type="number" step="0.01" className="form-input" required value={editForm.nivelCombustibleSalida} onChange={e => setEditForm({...editForm, nivelCombustibleSalida: e.target.value})} /></div>
              </div>
              <div className="form-group"><label className="form-label">Estado</label>
                <select className="form-input" value={editForm.estadoContrato} onChange={e => setEditForm({...editForm, estadoContrato: e.target.value})}>
                  <option value="ABIERTO">Abierto</option>
                  <option value="CERRADO">Cerrado</option>
                </select></div>
              <div className="form-group"><label className="form-label">Observaciones</label>
                <input className="form-input" value={editForm.observaciones} onChange={e => setEditForm({...editForm, observaciones: e.target.value})} /></div>
              <div className="modal__footer">
                <button type="button" className="btn btn--ghost" onClick={() => setShowEdit(false)}>Cancelar</button>
                <button type="submit" className="btn btn--primary" disabled={saving}>{saving ? 'Guardando...' : 'Guardar cambios'}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
