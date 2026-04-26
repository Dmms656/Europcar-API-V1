import { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { bookingApi } from '../../api/bookingApi';
import { Car, ArrowLeft, Fuel, Users, Settings2, MapPin, Filter } from 'lucide-react';

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

  useEffect(() => {
    loadLocalizaciones();
  }, []);

  useEffect(() => {
    buscar();
  }, [filtros.page]);

  const loadLocalizaciones = async () => {
    try {
      const res = await bookingApi.getLocalizaciones({});
      setLocalizaciones(res.data?.data?.localizaciones || []);
    } catch (e) { console.error(e); }
  };

  const buscar = async () => {
    setLoading(true);
    try {
      const params = { page: filtros.page, limit: filtros.limit };
      if (filtros.idLocalizacion) params.idLocalizacion = filtros.idLocalizacion;
      if (filtros.fechaRecogida) params.fechaRecogida = filtros.fechaRecogida;
      if (filtros.fechaDevolucion) params.fechaDevolucion = filtros.fechaDevolucion;
      const res = await bookingApi.buscarVehiculos(params);
      setVehiculos(res.data?.data?.vehiculos || []);
    } catch (e) {
      console.error(e);
      setVehiculos([]);
    } finally {
      setLoading(false);
    }
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
              <option value="">Todas</option>
              {localizaciones.map((l) => (
                <option key={l.idLocalizacion || l.id} value={l.idLocalizacion || l.id}>
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
          <button className="btn btn--primary btn--full" onClick={buscar}>Buscar</button>
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
                <div key={v.idVehiculo || v.vehiculoGuid} className="vehicle-card">
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
                      <span><Fuel size={14} /> {v.tipoCombustible}</span>
                      <span><Settings2 size={14} /> {v.tipoTransmision}</span>
                    </div>
                    <div className="vehicle-card__footer">
                      <div className="vehicle-card__price">
                        <span className="vehicle-card__price-amount">${Number(v.precioBaseDia || v.precioDia || 0).toFixed(2)}</span>
                        <span className="vehicle-card__price-unit">/día</span>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
