import { useState, useEffect, useMemo } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { vehiculosApi } from '../../api/vehiculosApi';
import { bookingApi } from '../../api/bookingApi';
import { useAuthStore } from '../../store/useAuthStore';
import {
  Car, Search, Users, Fuel, Settings2, MapPin,
  SlidersHorizontal, X, Star, ShieldCheck, Zap, ArrowRight, LogIn, Home
} from 'lucide-react';

const isValidImageUrl = (url) => url && (url.startsWith('http://') || url.startsWith('https://'));

export default function CatalogoPage() {
  const [vehiculos, setVehiculos] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [filtros, setFiltros] = useState({
    categoria: '',
    combustible: '',
    transmision: '',
    precioMin: '',
    precioMax: '',
  });
  const [showFilters, setShowFilters] = useState(false);
  const { isAuthenticated, userType } = useAuthStore();
  const navigate = useNavigate();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [vehRes, catRes] = await Promise.allSettled([
        vehiculosApi.getDisponibles(),
        bookingApi.getCategorias(),
      ]);
      if (vehRes.status === 'fulfilled') {
        setVehiculos(vehRes.value.data?.data || []);
      }
      if (catRes.status === 'fulfilled') {
        setCategorias(catRes.value.data?.data?.categorias || []);
      }
    } catch (e) {
      console.error('Error loading catalog:', e);
    } finally {
      setLoading(false);
    }
  };

  const filteredVehiculos = useMemo(() => {
    return vehiculos.filter((v) => {
      const search = searchTerm.toLowerCase();
      const matchSearch = !search ||
        (v.marca || '').toLowerCase().includes(search) ||
        (v.modelo || v.modeloVehiculo || '').toLowerCase().includes(search) ||
        (v.categoria || '').toLowerCase().includes(search);

      const matchCategoria = !filtros.categoria ||
        (v.categoria || '').toLowerCase() === filtros.categoria.toLowerCase();

      const matchCombustible = !filtros.combustible ||
        (v.tipoCombustible || '').toLowerCase() === filtros.combustible.toLowerCase();

      const matchTransmision = !filtros.transmision ||
        (v.tipoTransmision || '').toLowerCase() === filtros.transmision.toLowerCase();

      const precio = Number(v.precioBaseDia || v.precioDia || 0);
      const matchPrecioMin = !filtros.precioMin || precio >= Number(filtros.precioMin);
      const matchPrecioMax = !filtros.precioMax || precio <= Number(filtros.precioMax);

      return matchSearch && matchCategoria && matchCombustible && matchTransmision && matchPrecioMin && matchPrecioMax;
    });
  }, [vehiculos, searchTerm, filtros]);

  const handleReservar = (vehiculo) => {
    if (!isAuthenticated) {
      navigate('/login', { state: { from: { pathname: `/reservar/${vehiculo.idVehiculo}` } } });
    } else {
      navigate(`/reservar/${vehiculo.idVehiculo}`);
    }
  };

  const clearFilters = () => {
    setFiltros({ categoria: '', combustible: '', transmision: '', precioMin: '', precioMax: '' });
    setSearchTerm('');
  };

  const activeFilterCount = Object.values(filtros).filter(Boolean).length + (searchTerm ? 1 : 0);

  return (
    <div className="catalogo-page">
      {/* Navigation */}
      <nav className="home-nav home-nav--catalog">
        <div className="home-nav__inner">
          <Link to="/" className="home-nav__logo">
            <Car size={28} />
            <span>Europcar</span>
          </Link>
          <div className="home-nav__links">
            <Link to="/" className="home-nav__link"><Home size={16} /> Inicio</Link>
            <Link to="/catalogo" className="home-nav__link home-nav__link--active">Catálogo</Link>
            {isAuthenticated ? (
              <Link to={userType === 'admin' ? '/dashboard' : '/mi-cuenta'} className="home-nav__btn">
                {userType === 'admin' ? 'Panel Admin' : 'Mi Cuenta'}
              </Link>
            ) : (
              <Link to="/login" className="home-nav__btn"><LogIn size={16} /> Iniciar Sesión</Link>
            )}
          </div>
        </div>
      </nav>

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
              <label className="form-label">Categoría</label>
              <select className="form-input" value={filtros.categoria}
                onChange={(e) => setFiltros({ ...filtros, categoria: e.target.value })}>
                <option value="">Todas</option>
                {categorias.map((c) => (
                  <option key={c.id} value={c.nombre}>{c.nombre}</option>
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
