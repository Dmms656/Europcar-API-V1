import { useState, useEffect, useMemo } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { vehiculosApi } from '../../api/vehiculosApi';
import { bookingApi } from '../../api/bookingApi';
import { catalogosApi } from '../../api/catalogosApi';
import { useAuthStore } from '../../store/useAuthStore';
import {
  Car, Search, Users, Fuel, Settings2, MapPin,
  SlidersHorizontal, X, Star, ShieldCheck, Zap, ArrowRight, LogIn, Home
} from 'lucide-react';

const isValidImageUrl = (url) => url && (url.startsWith('http://') || url.startsWith('https://'));

export default function CatalogoPage() {
  const [vehiculos, setVehiculos] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [ciudades, setCiudades] = useState([]);
  const [localizaciones, setLocalizaciones] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [filtros, setFiltros] = useState({
    pais: '',
    ciudad: '',
    categoria: '',
    combustible: '',
    transmision: '',
    precioMin: '',
    precioMax: '',
  });
  const [showFilters, setShowFilters] = useState(false);
  const { isAuthenticated, userType } = useAuthStore();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  /** ?categoria=... desde la home u otros enlaces */
  useEffect(() => {
    const raw = searchParams.get('categoria');
    if (!raw) return;
    const decoded = decodeURIComponent(raw.trim());
    setFiltros((prev) => ({ ...prev, categoria: decoded }));
  }, [searchParams]);

  /** ?pais=... y ?ciudad=... desde HomePage */
  useEffect(() => {
    const pais = searchParams.get('pais') || '';
    const ciudad = searchParams.get('ciudad') || '';
    if (!pais && !ciudad) return;
    setFiltros((prev) => ({
      ...prev,
      pais: pais || prev.pais,
      ciudad: ciudad || prev.ciudad,
    }));
  }, [searchParams]);

  /** ?idCategoria=... — resolver al nombre cuando el listado ya cargó */
  useEffect(() => {
    const idParam = searchParams.get('idCategoria');
    if (!idParam || categorias.length === 0) return;
    const id = Number(idParam);
    if (Number.isNaN(id)) return;
    const found = categorias.find((c) => Number(c.id ?? c.idCategoria) === id);
    const nombre = found?.nombre ?? found?.nombreCategoria ?? '';
    if (nombre)
      setFiltros((prev) => ({ ...prev, categoria: nombre }));
  }, [categorias, searchParams]);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [vehRes, catRes, ciuRes, locRes] = await Promise.allSettled([
        vehiculosApi.getDisponibles(),
        bookingApi.getCategorias(),
        catalogosApi.getCiudades(),
        catalogosApi.getLocalizaciones(),
      ]);
      if (vehRes.status === 'fulfilled') {
        setVehiculos(vehRes.value.data?.data || []);
      }
      if (catRes.status === 'fulfilled') {
        setCategorias(catRes.value.data?.data?.categorias || []);
      }
      if (ciuRes.status === 'fulfilled') {
        setCiudades(ciuRes.value.data?.data || []);
      }
      if (locRes.status === 'fulfilled') {
        setLocalizaciones(locRes.value.data?.data || []);
      }
    } catch (e) {
      console.error('Error loading catalog:', e);
    } finally {
      setLoading(false);
    }
  };

  const ciudadesFiltradasPorPais = useMemo(() => {
    if (!filtros.pais) return ciudades;
    return ciudades.filter((c) => String(c.idPais) === String(filtros.pais));
  }, [ciudades, filtros.pais]);

  const paisesOptions = useMemo(() => {
    const map = new Map();
    ciudades.forEach((c) => {
      const idPais = String(c.idPais ?? '');
      if (!idPais) return;
      if (!map.has(idPais)) {
        map.set(idPais, c.nombrePais || `País ${idPais}`);
      }
    });
    return Array.from(map.entries())
      .map(([id, nombre]) => ({ id, nombre }))
      .sort((a, b) => a.nombre.localeCompare(b.nombre));
  }, [ciudades]);

  const filteredVehiculos = useMemo(() => {
    const ciudadById = new Map(
      ciudades.map((c) => [String(c.idCiudad), c]),
    );
    const locationByNombre = new Map(
      localizaciones.map((l) => [
        (l.nombreLocalizacion || '').toString().trim().toLowerCase(),
        l,
      ]),
    );

    return vehiculos.filter((v) => {
      const search = searchTerm.toLowerCase();
      const matchSearch = !search ||
        (v.marca || '').toLowerCase().includes(search) ||
        (v.modelo || v.modeloVehiculo || '').toLowerCase().includes(search) ||
        (v.categoria || '').toLowerCase().includes(search);

      const localizacionNombre = (v.localizacion || '').toString().trim().toLowerCase();
      const localizacion = locationByNombre.get(localizacionNombre);
      const ciudad = ciudadById.get(String(localizacion?.idCiudad ?? ''));

      const matchPais = !filtros.pais || String(ciudad?.idPais ?? '') === String(filtros.pais);
      const matchCiudad = !filtros.ciudad || String(localizacion?.idCiudad ?? '') === String(filtros.ciudad);

      const catFilter = (filtros.categoria || '').trim().toLowerCase();
      const matchCategoria =
        !catFilter
        || (v.categoria || v.nombreCategoria || '')
          .toString()
          .trim()
          .toLowerCase() === catFilter;

      const matchCombustible = !filtros.combustible ||
        (v.tipoCombustible || '').toLowerCase() === filtros.combustible.toLowerCase();

      const matchTransmision = !filtros.transmision ||
        (v.tipoTransmision || '').toLowerCase() === filtros.transmision.toLowerCase();

      const precio = Number(v.precioBaseDia || v.precioDia || 0);
      const matchPrecioMin = !filtros.precioMin || precio >= Number(filtros.precioMin);
      const matchPrecioMax = !filtros.precioMax || precio <= Number(filtros.precioMax);

      return matchSearch
        && matchPais
        && matchCiudad
        && matchCategoria
        && matchCombustible
        && matchTransmision
        && matchPrecioMin
        && matchPrecioMax;
    });
  }, [vehiculos, searchTerm, filtros, localizaciones, ciudades]);

  const handleReservar = (vehiculo) => {
    navigate(`/reservar/${vehiculo.idVehiculo}`);
  };

  const clearFilters = () => {
    setFiltros({
      pais: '',
      ciudad: '',
      categoria: '',
      combustible: '',
      transmision: '',
      precioMin: '',
      precioMax: '',
    });
    setSearchTerm('');
    const next = new URLSearchParams(searchParams);
    next.delete('categoria');
    next.delete('idCategoria');
    setSearchParams(next, { replace: true });
  };

  const activeFilterCount = Object.values(filtros).filter(Boolean).length + (searchTerm ? 1 : 0);

  return (
    <div className="catalogo-page">

      {/* Hero Banner */}
      <div className="catalog-hero">
        <div className="catalog-hero__content">
          <h1 className="catalog-hero__title">
            Encuentra tu vehículo <span className="text-accent">ideal</span>
          </h1>
          <p className="catalog-hero__subtitle">
            Explora nuestra flota premium y reserva en minutos
          </p>
          <div className="catalog-search">
            <Search size={20} className="catalog-search__icon" />
            <input
              type="text"
              className="catalog-search__input"
              placeholder="Buscar por marca, modelo o categoría..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
            <button
              className="catalog-search__filter-btn"
              onClick={() => setShowFilters(!showFilters)}
            >
              <SlidersHorizontal size={18} />
              Filtros
              {activeFilterCount > 0 && (
                <span className="catalog-search__badge">{activeFilterCount}</span>
              )}
            </button>
          </div>
        </div>
      </div>

      {/* Filter Panel */}
      {showFilters && (
        <div className="catalog-filters">
          <div className="catalog-filters__inner">
            <div className="catalog-filters__group">
              <label className="form-label">País</label>
              <select
                className="form-input"
                value={filtros.pais}
                onChange={(e) => {
                  const pais = e.target.value;
                  setFiltros({ ...filtros, pais, ciudad: '' });
                }}
              >
                <option value="">Todos</option>
                {paisesOptions.map((p) => (
                  <option key={p.id} value={p.id}>{p.nombre}</option>
                ))}
              </select>
            </div>
            <div className="catalog-filters__group">
              <label className="form-label">Ciudad</label>
              <select
                className="form-input"
                value={filtros.ciudad}
                onChange={(e) => setFiltros({ ...filtros, ciudad: e.target.value })}
              >
                <option value="">Todas</option>
                {ciudadesFiltradasPorPais.map((c) => (
                  <option key={c.idCiudad} value={c.idCiudad}>
                    {c.nombreCiudad}
                  </option>
                ))}
              </select>
            </div>
            <div className="catalog-filters__group">
              <label className="form-label">Categoría</label>
              <select
                className="form-input"
                value={filtros.categoria}
                onChange={(e) => {
                  const val = e.target.value;
                  setFiltros({ ...filtros, categoria: val });
                  const next = new URLSearchParams(searchParams);
                  if (val) next.set('categoria', val);
                  else next.delete('categoria');
                  next.delete('idCategoria');
                  setSearchParams(next, { replace: true });
                }}
              >
                <option value="">Todas</option>
                {categorias.map((c) => (
                  <option key={c.id ?? c.idCategoria} value={c.nombre ?? c.nombreCategoria ?? ''}>
                    {c.nombre ?? c.nombreCategoria}
                  </option>
                ))}
              </select>
            </div>
            <div className="catalog-filters__group">
              <label className="form-label">Combustible</label>
              <select className="form-input" value={filtros.combustible}
                onChange={(e) => setFiltros({ ...filtros, combustible: e.target.value })}>
                <option value="">Todos</option>
                <option value="GASOLINA">Gasolina</option>
                <option value="DIESEL">Diésel</option>
                <option value="HIBRIDO">Híbrido</option>
                <option value="ELECTRICO">Eléctrico</option>
              </select>
            </div>
            <div className="catalog-filters__group">
              <label className="form-label">Transmisión</label>
              <select className="form-input" value={filtros.transmision}
                onChange={(e) => setFiltros({ ...filtros, transmision: e.target.value })}>
                <option value="">Todas</option>
                <option value="AUTOMATICA">Automática</option>
                <option value="MANUAL">Manual</option>
              </select>
            </div>
            <div className="catalog-filters__group">
              <label className="form-label">Precio mín.</label>
              <input type="number" className="form-input" placeholder="$0"
                value={filtros.precioMin}
                onChange={(e) => setFiltros({ ...filtros, precioMin: e.target.value })} />
            </div>
            <div className="catalog-filters__group">
              <label className="form-label">Precio máx.</label>
              <input type="number" className="form-input" placeholder="$999"
                value={filtros.precioMax}
                onChange={(e) => setFiltros({ ...filtros, precioMax: e.target.value })} />
            </div>
            <button className="btn btn--ghost" onClick={clearFilters}>
              <X size={16} /> Limpiar
            </button>
          </div>
        </div>
      )}

      {/* Results */}
      <div className="catalog-content">
        <div className="catalog-results-header">
          <h2>{filteredVehiculos.length} vehículos disponibles</h2>
        </div>

        {loading ? (
          <div className="catalog-loading">
            <div className="catalog-loading__spinner" />
            <p>Cargando flota...</p>
          </div>
        ) : filteredVehiculos.length === 0 ? (
          <div className="catalog-empty">
            <Car size={64} />
            <h3>No se encontraron vehículos</h3>
            <p>Intenta ajustar los filtros de búsqueda</p>
            <button className="btn btn--primary" onClick={clearFilters}>Limpiar filtros</button>
          </div>
        ) : (
          <div className="catalog-grid">
            {filteredVehiculos.map((v) => (
              <div key={v.idVehiculo || v.vehiculoGuid} className="catalog-card">
                <div className="catalog-card__image">
                  {isValidImageUrl(v.imagenUrl) ? (
                    <img src={v.imagenUrl} alt={`${v.marca} ${v.modelo || v.modeloVehiculo}`} />
                  ) : (
                    <div className="catalog-card__image-placeholder">
                      <Car size={56} />
                      <span>{v.marca} {v.modelo || v.modeloVehiculo}</span>
                    </div>
                  )}
                  <div className="catalog-card__badges">
                    <span className="catalog-card__badge catalog-card__badge--category">
                      {v.categoria || v.categoriaVehiculo}
                    </span>
                    {(v.tipoCombustible === 'ELECTRICO' || v.tipoCombustible === 'HIBRIDO') && (
                      <span className="catalog-card__badge catalog-card__badge--eco">
                        <Zap size={12} /> Eco
                      </span>
                    )}
                  </div>
                </div>

                <div className="catalog-card__body">
                  <div className="catalog-card__header">
                    <h3 className="catalog-card__title">
                      {v.marca} {v.modelo || v.modeloVehiculo}
                    </h3>
                    <span className="catalog-card__year">{v.anioFabricacion}</span>
                  </div>

                  <div className="catalog-card__specs">
                    <div className="catalog-card__spec">
                      <Users size={15} />
                      <span>{v.capacidadPasajeros} pasajeros</span>
                    </div>
                    <div className="catalog-card__spec">
                      <Fuel size={15} />
                      <span>{v.tipoCombustible}</span>
                    </div>
                    <div className="catalog-card__spec">
                      <Settings2 size={15} />
                      <span>{v.tipoTransmision}</span>
                    </div>
                    <div className="catalog-card__spec">
                      <MapPin size={15} />
                      <span>{v.localizacion}</span>
                    </div>
                  </div>

                  <div className="catalog-card__features">
                    {v.aireAcondicionado && (
                      <span className="catalog-card__feature">
                        <ShieldCheck size={14} /> A/C
                      </span>
                    )}
                    <span className="catalog-card__feature">
                      <Star size={14} /> {v.capacidadMaletas} maletas
                    </span>
                  </div>

                  <div className="catalog-card__footer">
                    <div className="catalog-card__price">
                      <span className="catalog-card__price-amount">
                        ${Number(v.precioBaseDia || v.precioDia || 0).toFixed(2)}
                      </span>
                      <span className="catalog-card__price-unit">/día</span>
                    </div>
                    <button
                      className="btn btn--primary catalog-card__btn"
                      onClick={() => handleReservar(v)}
                    >
                      Reservar <ArrowRight size={16} />
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
