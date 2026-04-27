import { Link } from 'react-router-dom';
import { Compass, Home, ArrowLeft } from 'lucide-react';

export default function NotFoundPage() {
  return (
    <div className="error-page">
      <div className="error-page__card">
        <div className="error-page__icon">
          <Compass size={48} />
        </div>
        <h1 className="error-page__code">404</h1>
        <h2 className="error-page__title">Página no encontrada</h2>
        <p className="error-page__message">
          La página que buscas no existe o fue movida. Verifica la dirección o
          regresa al inicio.
        </p>
        <div className="error-page__actions">
          <Link to="/" className="btn btn--primary">
            <Home size={16} /> Inicio
          </Link>
          <button className="btn btn--secondary" onClick={() => window.history.back()}>
            <ArrowLeft size={16} /> Volver
          </button>
        </div>
      </div>
    </div>
  );
}
