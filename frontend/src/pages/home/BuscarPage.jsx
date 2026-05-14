import { useState, useEffect, useCallback } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { bookingApi } from '../../api/bookingApi';
import { Car, ArrowLeft, Fuel, Users, Settings2, Filter } from 'lucide-react';
import {
  defaultRentalDateTimeLocalRange,
  normalizeVehiculoFromBookingList,
} from '../../utils/bookingNormalize';

export default function BuscarPage() {
  const [searchParams] = useSearchParams();
  const [vehiculos, setVehiculos] = useState([]);
  const [localizaciones, setLocalizaciones] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filtros, setFiltros] = useState({
    idLocalizacion: searchParams.get('localizacion') || '',
    fechaRecogida: searchParams.get('fechaRecogida') || '',
    fechaDevolucion: searchParams.get('fechaDevolucion') || '',
    page: 1,
    limit: 12,
  });

  const runBuscar = useCallback(async (f) => {
    if (!f.idLocalizacion || !f.fechaRecogida || !f.fechaDevolucion) {
      setVehiculos([]);
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      const res = await bookingApi.buscarVehiculos({
        idLocalizacion: Number(f.idLocalizacion),
        fechaRecogida: f.fechaRecogida,
        fechaDevolucion: f.fechaDevolucion,
        page: f.page,
        limit: f.limit,
      });
      const raw = res.data?.data?.vehiculos || [];
      setVehiculos(raw.map(normalizeVehiculoFromBookingList));
    } catch (e) {
      console.error(e);
      setVehiculos([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const res = await bookingApi.getLocalizaciones({ page: 1, limit: 200 });
        if (cancelled) return;
        const locs = res.data?.data?.localizaciones || [];
        setLocalizaciones(locs);
        const def = defaultRentalDateTimeLocalRange();
        const idFromUrl = searchParams.get('localizacion');
        const firstId = locs[0] ? String(locs[0].idLocalizacion || locs[0].id) : '';
        const idLoc = idFromUrl || firstId;
        const fr = searchParams.get('fechaRecogida') || def.fechaRecogida;
        const fd = searchParams.get('fechaDevolucion') || def.fechaDevolucion;
        const next = {
          idLocalizacion: idLoc,
          fechaRecogida: fr,
          fechaDevolucion: fd,
          page: 1,
          limit: 12,
        };
        setFiltros(next);
        await runBuscar(next);
      } catch (e) {
        console.error(e);
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [runBuscar, searchParams]);

  const onBuscarClick = () => {
    runBuscar(filtros);
  };

  return (
    <div className="search-page">
      <nav className="home-nav">
        <div className="home-nav__inner">
          <Link to="/" className="home-nav__logo">
            <Car size={28} />
            <span>Europcar</span>
          </Link>
          <Link to="/login" className="home-nav__btn">Acceso Admin</Link>
        </div>
      </nav>

      <div className="search-page__content">
        <div className="search-filters">
          <h3><Filter size={18} /> Filtros</h3>
          <div className="form-group">
            <label className="form-label">Sucursal</label>
            <select className="form-input" value={filtros.idLocalizacion}
              onChange={(e) => setFiltros({ ...filtros, idLocalizacion: e.target.value })}>
              <option value="">Seleccione sucursal</option>
              {localizaciones.map((l) => (
                <option key={l.idLocalizacion || l.id} value={String(l.idLocalizacion || l.id)}>
                  {l.nombreLocalizacion || l.nombre}
                </option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">Recogida</label>
            <input type="datetime-local" className="form-input" value={filtros.fechaRecogida}
              onChange={(e) => setFiltros({ ...filtros, fechaRecogida: e.target.value })} />
          </div>
          <div className="form-group">
            <label className="form-label">Devolución</label>
            <input type="datetime-local" className="form-input" value={filtros.fechaDevolucion}
              onChange={(e) => setFiltros({ ...filtros, fechaDevolucion: e.target.value })} />
          </div>
          <button type="button" className="btn btn--primary btn--full" onClick={onBuscarClick}>Buscar</button>
        </div>

        <div className="search-results">
          <h2>Vehículos disponibles</h2>
          {loading ? (
            <div className="home-loading">Buscando vehículos...</div>
          ) : vehiculos.length === 0 ? (
            <div className="home-loading">No se encontraron vehículos con los filtros seleccionados.</div>
          ) : (
            <div className="vehicle-grid">
              {vehiculos.map((v) => (
                <Link key={v.idVehiculo || v.vehiculoGuid} to={`/reservar/${v.idVehiculo}`} className="vehicle-card vehicle-card--link">
                  <div className="vehicle-card__img">
                    {v.imagenUrl || v.imagenReferencialUrl ? (
                      <img src={v.imagenUrl || v.imagenReferencialUrl} alt={v.modelo} />
                    ) : (
                      <div className="vehicle-card__img-placeholder"><Car size={48} /></div>
                    )}
                    <span className="vehicle-card__badge">{v.categoria || v.categoriaVehiculo}</span>
                  </div>
                  <div className="vehicle-card__body">
                    <h3 className="vehicle-card__title">{v.marca} {v.modelo || v.modeloVehiculo}</h3>
                    <div className="vehicle-card__specs">
                      <span><Users size={14} /> {v.capacidadPasajeros} pax</span>
                      <span><Fuel size={14} /> {v.tipoCombustible || v.combustible || '—'}</span>
                      <span><Settings2 size={14} /> {v.tipoTransmision || v.transmision || '—'}</span>
                    </div>
                    <div className="vehicle-card__footer">
                      <div className="vehicle-card__price">
                        <span className="vehicle-card__price-amount">${Number(v.precioBaseDia || v.precioDia || 0).toFixed(2)}</span>
                        <span className="vehicle-card__price-unit">/día</span>
                      </div>
                    </div>
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
