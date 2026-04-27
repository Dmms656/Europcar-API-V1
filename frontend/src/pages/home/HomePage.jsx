import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { bookingApi } from '../../api/bookingApi';
import {
  Car, MapPin, Search, Shield, Star,
  ChevronRight, LogIn, Fuel, Users, Settings2, ShoppingBag, ArrowRight, Zap, ShieldCheck
} from 'lucide-react';
import DateTimePicker from '../../components/ui/DateTimePicker';

const isValidImageUrl = (url) => url && (url.startsWith('http://') || url.startsWith('https://'));

export default function HomePage() {
  const navigate = useNavigate();
  const [localizaciones, setLocalizaciones] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [vehiculosDestacados, setVehiculosDestacados] = useState([]);
  const [loading, setLoading] = useState(true);

  const [searchForm, setSearchForm] = useState({
    idLocalizacion: '',
    fechaRecogida: '',
    fechaDevolucion: '',
  });

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      const [locRes, catRes, vehRes] = await Promise.allSettled([
        bookingApi.getLocalizaciones({}),
        bookingApi.getCategorias(),
        bookingApi.buscarVehiculos({ page: 1, limit: 5 }),
      ]);
      if (locRes.status === 'fulfilled') setLocalizaciones(locRes.value.data?.data?.localizaciones || []);
      if (catRes.status === 'fulfilled') setCategorias(catRes.value.data?.data?.categorias || []);
      if (vehRes.status === 'fulfilled') {
        const vehiculos = vehRes.value.data?.data?.vehiculos || [];
        setVehiculosDestacados(vehiculos.slice(0, 5));
      }
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
    navigate(`/catalogo?${params.toString()}`);
  };

  return (
    <div className="home">
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
            <div className="hero-search__field hero-search__field--datetime">
              <DateTimePicker
                id="hero-recogida"
                label="Recogida"
                value={searchForm.fechaRecogida}
                onChange={(val) => setSearchForm({ ...searchForm, fechaRecogida: val })}
              />
            </div>
            <div className="hero-search__field hero-search__field--datetime">
              <DateTimePicker
                id="hero-devolucion"
                label="Devolución"
                value={searchForm.fechaDevolucion}
                minDate={searchForm.fechaRecogida}
                onChange={(val) => setSearchForm({ ...searchForm, fechaDevolucion: val })}
              />
            </div>
            <button type="submit" className="hero-search__btn">
              <Search size={18} />
              Buscar
            </button>
          </form>

          {/* CTA Buttons */}
          <div className="hero__ctas">
            <Link to="/catalogo" className="btn btn--primary btn--lg hero__cta">
              <ShoppingBag size={20} /> Ver Catálogo Completo
            </Link>
            <Link to="/login" className="btn btn--outline btn--lg hero__cta">
              <LogIn size={20} /> Acceder a tu Cuenta
            </Link>
          </div>
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

      {/* Fleet */}
      <section className="home-section" id="vehiculos">
        <div className="home-section__inner">
          <div className="home-section__header">
            <h2>Nuestra Flota</h2>
            <p>Conoce los primeros vehículos disponibles de nuestro catálogo</p>
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
            <>
              <div className="catalog-grid">
                {vehiculosDestacados.map((v) => (
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
                        <h3 className="catalog-card__title">{v.marca} {v.modelo || v.modeloVehiculo}</h3>
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
                          onClick={() => navigate(`/reservar/${v.idVehiculo}`)}
                        >
                          Reservar <ArrowRight size={16} />
                        </button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
              <div className="home-section__cta">
                <Link to="/catalogo" className="btn btn--outline btn--lg">
                  Ver todo el catálogo <ChevronRight size={18} />
                </Link>
              </div>
            </>
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
              <p>Explora el <Link to="/catalogo">catálogo</Link> y filtra por marca, categoría o precio</p>
            </div>
            <div className="step-card">
              <div className="step-card__number">2</div>
              <div className="step-card__icon"><Car size={28} /></div>
              <h3>Reserva</h3>
              <p>Selecciona fechas, agrega extras y realiza tu pago en línea</p>
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
                <Link to={`/catalogo?categoria=${cat.nombreCategoria || cat.nombre}`}
                  key={cat.idCategoria || cat.id} className="category-card">
                  <Car size={32} />
                  <h3>{cat.nombreCategoria || cat.nombre}</h3>
                  {cat.descripcion && <p>{cat.descripcion}</p>}
                </Link>
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
            <Link to="/catalogo">Catálogo</Link>
            <Link to="/login">Iniciar Sesión</Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
