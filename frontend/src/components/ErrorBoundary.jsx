import { Component } from 'react';
import { AlertTriangle, RefreshCw, Home } from 'lucide-react';

/**
 * ErrorBoundary global. Atrapa errores de render en cualquier hijo y muestra
 * un fallback elegante con opciones de reintentar / volver al inicio.
 *
 * Uso:
 *   <ErrorBoundary>
 *     <App />
 *   </ErrorBoundary>
 */
export default class ErrorBoundary extends Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }

  componentDidCatch(error, info) {
    if (import.meta.env.DEV) {
      console.error('[ErrorBoundary]', error, info);
    }
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null });
  };

  handleReload = () => {
    window.location.reload();
  };

  handleHome = () => {
    window.location.href = '/';
  };

  render() {
    if (!this.state.hasError) return this.props.children;

    const isDev = import.meta.env.DEV;
    const message = this.state.error?.message || 'Ha ocurrido un error inesperado.';

    return (
      <div className="error-boundary">
        <div className="error-boundary__card">
          <div className="error-boundary__icon">
            <AlertTriangle size={48} />
          </div>
          <h1 className="error-boundary__title">Algo salió mal</h1>
          <p className="error-boundary__message">
            La aplicación encontró un problema inesperado. Puedes intentar recargar la
            página o volver al inicio.
          </p>

          {isDev && (
            <pre className="error-boundary__detail">{message}</pre>
          )}

          <div className="error-boundary__actions">
            <button className="btn btn--primary" onClick={this.handleReload}>
              <RefreshCw size={16} /> Recargar
            </button>
            <button className="btn btn--secondary" onClick={this.handleHome}>
              <Home size={16} /> Ir al inicio
            </button>
          </div>
        </div>
      </div>
    );
  }
}
