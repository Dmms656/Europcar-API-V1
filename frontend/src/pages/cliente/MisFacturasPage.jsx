import { useEffect, useState } from 'react';
import { FileText, Search, Loader2, Inbox } from 'lucide-react';
import { toast } from 'sonner';
import { facturasApi } from '../../api/facturasApi';

const estadoClass = (estado) => {
  if (estado === 'PAGADA') return 'status-badge--success';
  if (estado === 'ANULADA') return 'status-badge--danger';
  return 'status-badge--warning';
};

export default function MisFacturasPage() {
  const [facturas, setFacturas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  useEffect(() => {
    loadFacturas();
  }, []);

  const loadFacturas = async () => {
    setLoading(true);
    try {
      const res = await facturasApi.getMyFacturas();
      const data = res.data?.data;
      setFacturas(Array.isArray(data) ? data : []);
    } catch (err) {
      toast.error(err.response?.data?.message || 'Error al cargar facturas');
      setFacturas([]);
    } finally {
      setLoading(false);
    }
  };

  const filtered = facturas.filter((f) => {
    const text = `${f.numeroFactura || ''} ${f.codigoReserva || ''} ${f.numeroContrato || ''} ${f.estadoFactura || ''}`.toLowerCase();
    return text.includes(search.toLowerCase());
  });

  return (
    <div className="module-page mis-facturas-page">
      <div className="module-page__header">
        <div>
          <h1><FileText size={24} /> Mis Facturas</h1>
          <p>{facturas.length} facturas emitidas</p>
        </div>
      </div>

      <div className="module-page__toolbar">
        <div className="search-box">
          <Search size={16} />
          <input
            placeholder="Buscar por número, reserva, contrato o estado..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </div>

      {loading ? (
        <div className="module-loading"><Loader2 size={24} className="spin" /> Cargando facturas...</div>
      ) : filtered.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '3rem 1rem', opacity: 0.7 }}>
          <Inbox size={48} />
          <p style={{ marginTop: '1rem', fontSize: '1.05rem' }}>No tienes facturas para mostrar.</p>
        </div>
      ) : (
        <div className="data-table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>Factura</th>
                <th>Fecha</th>
                <th>Reserva</th>
                <th>Contrato</th>
                <th>Subtotal</th>
                <th>IVA</th>
                <th>Total</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((f) => (
                <tr key={f.idFactura}>
                  <td><code>{f.numeroFactura}</code></td>
                  <td>{f.fechaEmision ? new Date(f.fechaEmision).toLocaleDateString('es-EC') : '-'}</td>
                  <td><code>{f.codigoReserva || '-'}</code></td>
                  <td><code>{f.numeroContrato || '-'}</code></td>
                  <td>${Number(f.subtotal || 0).toFixed(2)}</td>
                  <td>${Number(f.valorIva || 0).toFixed(2)}</td>
                  <td><strong>${Number(f.total || 0).toFixed(2)}</strong></td>
                  <td>
                    <span className={`status-badge ${estadoClass(f.estadoFactura)}`}>
                      {f.estadoFactura}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
