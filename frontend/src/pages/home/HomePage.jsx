import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { bookingApi } from '../../api/bookingApi';
import {
  Car, MapPin, Search, Shield, LogIn, ShoppingBag
} from 'lucide-react';
import DateTimePicker from '../../components/ui/DateTimePicker';

export default function HomePage() {
  const navigate = useNavigate();
  const [localizaciones, setLocalizaciones] = useState([]);
  const [ciudades, setCiudades] = useState([]);
  const [categorias, setCategorias] = useState([]);

  const [searchForm, setSearchForm] = useState({
    idPais: '',
    idCiudad: '',
    idLocalizacion: '',
    fechaRecogida: '',
    fechaDevolucion: '',
  });

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      const [locRes, catRes, ciuRes] = await Promise.allSettled([
        bookingApi.getLocalizaciones({}),
        bookingApi.getCategorias(),
        bookingApi.getCiudades(),
      ]);
      if (locRes.status === 'fulfilled') setLocalizaciones(locRes.value.data?.data?.localizaciones || []);
      if (catRes.status === 'fulfilled') setCategorias(catRes.value.data?.data?.categorias || []);
      if (ciuRes.status === 'fulfilled') setCiudades(ciuRes.value.data?.data?.ciudades || []);
    } catch (e) {
      console.error('Error loading homepage data:', e);
    }
  };

  const paises = Array.from(
    ciudades.reduce((acc, c) => {
      acc.set(String(c.idPais), c.nombrePais);
      return acc;
    }, new Map()),
  ).map(([idPais, nombrePais]) => ({ idPais, nombrePais }));

  const ciudadesFiltradas = searchForm.idPais
    ? ciudades.filter((c) => String(c.idPais) === String(searchForm.idPais))
    : ciudades;

  const localizacionesFiltradas = localizaciones.filter((loc) => {
    if (!searchForm.idCiudad) return true;
    const ciudadId = loc.ciudad?.id || loc.idCiudad;
    return String(ciudadId) === String(searchForm.idCiudad);
  });

  const handleSearch = (e) => {
    e.preventDefault();
    const params = new URLSearchParams();
    if (searchForm.idPais) params.set('pais', searchForm.idPais);
    if (searchForm.idCiudad) params.set('ciudad', searchForm.idCiudad);
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
                value={searchForm.idPais}
                onChange={(e) => setSearchForm({
                  ...searchForm, idPais: e.target.value, idCiudad: '', idLocalizacion: '',
                })}
                className="hero-search__select"
              >
                <option value="">Todos los países</option>
                {paises.map((p) => (
                  <option key={p.idPais} value={p.idPais}>{p.nombrePais}</option>
                ))}
              </select>
            </div>
            <div className="hero-search__field">
              <MapPin size={18} className="hero-search__icon" />
              <select
                value={searchForm.idCiudad}
                onChange={(e) => setSearchForm({ ...searchForm, idCiudad: e.target.value, idLocalizacion: '' })}
                className="hero-search__select"
              >
                <option value="">Todas las ciudades</option>
                {ciudadesFiltradas.map((c) => (
                  <option key={c.idCiudad} value={c.idCiudad}>
                    {c.nombreCiudad}{c.nombrePais ? ` · ${c.nombrePais}` : ''}
                  </option>
                ))}
              </select>
            </div>
            <div className="hero-search__field">
              <MapPin size={18} className="hero-search__icon" />
              <select
                value={searchForm.idLocalizacion}
                onChange={(e) => setSearchForm({ ...searchForm, idLocalizacion: e.target.value })}
                className="hero-search__select"
              >
                <option value="">Todas las sucursales</option>
                {localizacionesFiltradas.map((loc) => (
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
                <Link
                  to={`/catalogo?categoria=${encodeURIComponent(cat.nombre || cat.nombreCategoria || '')}`}
                  key={cat.idCategoria || cat.id}
                  className="category-card"
                >
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
