import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { bookingApi } from '../../api/bookingApi';
import { Car, MapPin, Calendar, Search, Shield, Clock, Star, ChevronRight, LogIn, Fuel, Users, Settings2 } from 'lucide-react';

export default function HomePage() {
  const navigate = useNavigate();
  const [localizaciones, setLocalizaciones] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [vehiculosDestacados, setVehiculosDestacados] = useState([]);
  const [loading, setLoading] = useState(true);

  // Search form state
  const [searchForm, setSearchForm] = useState({
    idLocalizacion: '',
    fechaRecogida: '',
    fechaDevolucion: '',
  });

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [locRes, catRes, vehRes] = await Promise.allSettled([
        bookingApi.getLocalizaciones({}),
        bookingApi.getCategorias(),
        bookingApi.buscarVehiculos({ page: 1, limit: 6 }),
      ]);
      if (locRes.status === 'fulfilled') setLocalizaciones(locRes.value.data?.data?.localizaciones || []);
      if (catRes.status === 'fulfilled') setCategorias(catRes.value.data?.data?.categorias || []);
      if (vehRes.status === 'fulfilled') setVehiculosDestacados(vehRes.value.data?.data?.vehiculos || []);
    } catch (e) {
      console.error('Error loading homepage data:', e);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (e) => {
    e.preventDefault();
    const params = new URLSearchParams();
    if (searchForm.idLocalizacion) params.set('localizacion', searchForm.idLocalizacion);
    if (searchForm.fechaRecogida) params.set('fechaRecogida', searchForm.fechaRecogida);
    if (searchForm.fechaDevolucion) params.set('fechaDevolucion', searchForm.fechaDevolucion);
    navigate(`/buscar?${params.toString()}`);
  };

  return (
    <div className="home">
      {/* Navbar */}
      <nav className="home-nav">
        <div className="home-nav__inner">
          <div className="home-nav__logo">
            <Car size={28} />
            <span>Europcar</span>
          </div>
          <div className="home-nav__links">
            <a href="#vehiculos" className="home-nav__link">Vehículos</a>
            <a href="#como-funciona" className="home-nav__link">Cómo funciona</a>
            <Link to="/login" className="home-nav__btn">
              <LogIn size={16} />
              Acceso Admin
            </Link>
          </div>
        </div>
      </nav>

      {/* Hero Section */}
      <section className="hero">
        <div className="hero__bg" />
        <div className="hero__content">
          <span className="hero__badge">🚗 La mejor experiencia en renta de vehículos</span>
          <h1 className="hero__title">
            Renta tu vehículo <br />
            <span className="hero__title-accent">ideal hoy</span>
          </h1>
          <p className="hero__subtitle">
            Amplia flota de vehículos, precios competitivos y servicio premium.
            Recoge y devuelve en múltiples sucursales.
          </p>

          {/* Search Form */}
          <form className="hero-search" onSubmit={handleSearch}>
            <div className="hero-search__field">
              <MapPin size={18} className="hero-search__icon" />
              <select
                value={searchForm.idLocalizacion}
                onChange={(e) => setSearchForm({ ...searchForm, idLocalizacion: e.target.value })}
                className="hero-search__select"
              >
                <option value="">Todas las sucursales</option>
                {localizaciones.map((loc) => (
                  <option key={loc.idLocalizacion || loc.id} value={loc.idLocalizacion || loc.id}>
                    {loc.nombreLocalizacion || loc.nombre}
                  </option>
                ))}
              </select>
            </div>
            <div className="hero-search__field">
              <Calendar size={18} className="hero-search__icon" />
              <input
                type="datetime-local"
                className="hero-search__input"
                placeholder="Fecha recogida"
                value={searchForm.fechaRecogida}
                onChange={(e) => setSearchForm({ ...searchForm, fechaRecogida: e.target.value })}
              />
            </div>
            <div className="hero-search__field">
              <Calendar size={18} className="hero-search__icon" />
              <input
                type="datetime-local"
                className="hero-search__input"
                placeholder="Fecha devolución"
                value={searchForm.fechaDevolucion}
                onChange={(e) => setSearchForm({ ...searchForm, fechaDevolucion: e.target.value })}
              />
            </div>
            <button type="submit" className="hero-search__btn">
              <Search size={18} />
              Buscar
            </button>
          </form>
        </div>
      </section>

      {/* Stats */}
      <section className="home-stats">
        <div className="home-stats__inner">
          <div className="home-stat">
            <span className="home-stat__number">50+</span>
            <span className="home-stat__label">Vehículos</span>
          </div>
          <div className="home-stat">
            <span className="home-stat__number">5</span>
            <span className="home-stat__label">Sucursales</span>
          </div>
          <div className="home-stat">
            <span className="home-stat__number">24/7</span>
            <span className="home-stat__label">Soporte</span>
          </div>
          <div className="home-stat">
            <span className="home-stat__number">4.8</span>
            <span className="home-stat__label">Calificación</span>
          </div>
        </div>
      </section>

      {/* Featured Vehicles */}
      <section className="home-section" id="vehiculos">
        <div className="home-section__inner">
          <div className="home-section__header">
            <h2>Vehículos Disponibles</h2>
            <p>Explora nuestra flota y encuentra el vehículo perfecto para tu viaje</p>
          </div>

          {loading ? (
            <div className="home-loading">Cargando vehículos...</div>
          ) : vehiculosDestacados.length === 0 ? (
            <div className="home-loading">
              <p>Conectando con la API...</p>
              <p style={{fontSize:'0.8rem',color:'var(--color-text-muted)',marginTop:'0.5rem'}}>
                La API en Render puede tardar ~30s en despertar si estaba inactiva.
              </p>
            </div>
          ) : (
            <div className="vehicle-grid">
              {vehiculosDestacados.map((v) => (
                <div key={v.idVehiculo || v.vehiculoGuid} className="vehicle-card">
                  <div className="vehicle-card__img">
                    {v.imagenUrl || v.imagenReferencialUrl ? (
                      <img src={v.imagenUrl || v.imagenReferencialUrl} alt={v.modelo} />
                    ) : (
                      <div className="vehicle-card__img-placeholder">
                        <Car size={48} />
                      </div>
                    )}
                    <span className="vehicle-card__badge">{v.categoria || v.categoriaVehiculo}</span>
                  </div>
                  <div className="vehicle-card__body">
                    <h3 className="vehicle-card__title">
                      {v.marca || v.marcaVehiculo} {v.modelo || v.modeloVehiculo}
                    </h3>
                    <p className="vehicle-card__year">{v.anioFabricacion}</p>
                    <div className="vehicle-card__specs">
                      <span><Users size={14} /> {v.capacidadPasajeros} pax</span>
                      <span><Fuel size={14} /> {v.tipoCombustible}</span>
                      <span><Settings2 size={14} /> {v.tipoTransmision}</span>
                    </div>
                    <div className="vehicle-card__footer">
                      <div className="vehicle-card__price">
                        <span className="vehicle-card__price-amount">
                          ${Number(v.precioBaseDia || v.precioDia || 0).toFixed(2)}
                        </span>
                        <span className="vehicle-card__price-unit">/día</span>
                      </div>
                      <button className="btn btn--primary btn--sm" onClick={() => navigate(`/buscar`)}>
                        Reservar
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </section>

      {/* How it works */}
      <section className="home-section home-section--alt" id="como-funciona">
        <div className="home-section__inner">
          <div className="home-section__header">
            <h2>¿Cómo funciona?</h2>
            <p>Renta tu vehículo en 3 simples pasos</p>
          </div>
          <div className="steps-grid">
            <div className="step-card">
              <div className="step-card__number">1</div>
              <div className="step-card__icon"><Search size={28} /></div>
              <h3>Busca</h3>
              <p>Selecciona tu sucursal, fechas y tipo de vehículo</p>
            </div>
            <div className="step-card">
              <div className="step-card__number">2</div>
              <div className="step-card__icon"><Car size={28} /></div>
              <h3>Elige</h3>
              <p>Compara opciones y selecciona el vehículo ideal</p>
            </div>
            <div className="step-card">
              <div className="step-card__number">3</div>
              <div className="step-card__icon"><Shield size={28} /></div>
              <h3>Disfruta</h3>
              <p>Recoge tu vehículo y disfruta tu viaje con total seguridad</p>
            </div>
          </div>
        </div>
      </section>

      {/* Categories */}
      {categorias.length > 0 && (
        <section className="home-section">
          <div className="home-section__inner">
            <div className="home-section__header">
              <h2>Categorías de Vehículos</h2>
              <p>Tenemos el vehículo perfecto para cada necesidad</p>
            </div>
            <div className="category-grid">
              {categorias.map((cat) => (
                <div key={cat.idCategoria || cat.id} className="category-card">
                  <Car size={32} />
                  <h3>{cat.nombreCategoria || cat.nombre}</h3>
                  {cat.descripcion && <p>{cat.descripcion}</p>}
                </div>
              ))}
            </div>
          </div>
        </section>
      )}

      {/* Footer */}
      <footer className="home-footer">
        <div className="home-footer__inner">
          <div className="home-footer__brand">
            <Car size={24} />
            <span>Europcar Rental</span>
          </div>
          <p className="home-footer__text">
            Sistema de gestión de renta de vehículos © {new Date().getFullYear()}
          </p>
          <div className="home-footer__links">
            <Link to="/login">Acceso Administrativo</Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
