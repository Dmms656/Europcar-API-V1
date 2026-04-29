import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Toaster, toast } from 'sonner';
import {
  QueryClient,
  QueryClientProvider,
  QueryCache,
  MutationCache,
} from '@tanstack/react-query';
import MainLayout from './components/layout/MainLayout';
import ClienteLayout from './components/layout/ClienteLayout';
import Navbar from './components/layout/Navbar';
import { ProtectedRoute } from './routes/ProtectedRoute';
import ErrorBoundary from './components/ErrorBoundary';
import { parseApiError } from './utils/errorHandler';

// Public pages
import HomePage from './pages/home/HomePage';
import BuscarPage from './pages/home/BuscarPage';
import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import CatalogoPage from './pages/catalogo/CatalogoPage';
import ReservarPage from './pages/reservar/ReservarPage';
import NotFoundPage from './pages/NotFoundPage';

// Admin pages
import DashboardPage from './pages/dashboard/DashboardPage';
import ClientesPage from './pages/clientes/ClientesPage';
import VehiculosPage from './pages/vehiculos/VehiculosPage';
import ReservasPage from './pages/reservas/ReservasPage';
import ContratosPage from './pages/contratos/ContratosPage';
import PagosPage from './pages/pagos/PagosPage';
import MantenimientosPage from './pages/mantenimientos/MantenimientosPage';
import UsuariosPage from './pages/usuarios/UsuariosPage';
import LocalizacionesPage from './pages/localizaciones/LocalizacionesPage';
import ExtrasPage from './pages/extras/ExtrasPage';
import UbicacionesPage from './pages/ubicaciones/UbicacionesPage';

// Client portal pages
import MiCuentaPage from './pages/cliente/MiCuentaPage';
import MisReservasPage from './pages/cliente/MisReservasPage';
import MisContratosPage from './pages/cliente/MisContratosPage';
import MisFacturasPage from './pages/cliente/MisFacturasPage';
import HistorialPage from './pages/cliente/HistorialPage';

// QueryClient con manejo robusto: retry inteligente, no-spam de toasts y
// notificación global cuando una mutación falla sin que la pantalla la maneje.
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: (failureCount, error) => {
        const status = error?.response?.status;
        // No reintentes errores del cliente (4xx)
        if (status && status >= 400 && status < 500) return false;
        return failureCount < 3;
      },
      retryDelay: (attempt) => Math.min(1200 * 2 ** attempt, 10000),
      refetchOnWindowFocus: false,
      staleTime: 60_000,
    },
    mutations: {
      retry: false,
    },
  },
  // Toast genérico solo para errores que NADIE manejó (sin onError local).
  queryCache: new QueryCache({
    onError: (error, query) => {
      if (query.meta?.suppressErrorToast) return;
      // axiosClient ya muestra el toast; aquí evitamos doble notificación.
    },
  }),
  mutationCache: new MutationCache({
    onError: (error, _vars, _ctx, mutation) => {
      if (mutation.options.onError) return; // la pantalla la maneja
      if (mutation.meta?.suppressErrorToast) return;
      const parsed = parseApiError(error);
      toast.error(parsed.message);
    },
  }),
});

function WithNavbar({ children }) {
  return (
    <>
      <Navbar />
      {children}
    </>
  );
}

export default function App() {
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <Toaster
            position="top-right"
            toastOptions={{
              style: {
                background: 'var(--color-surface)',
                color: 'var(--color-text)',
                border: '1px solid var(--color-border)',
              },
            }}
            richColors
            closeButton
          />
          <Routes>
            {/* Public routes — all with Navbar */}
            <Route path="/" element={<WithNavbar><HomePage /></WithNavbar>} />
            <Route path="/buscar" element={<WithNavbar><BuscarPage /></WithNavbar>} />
            <Route path="/catalogo" element={<WithNavbar><CatalogoPage /></WithNavbar>} />
            <Route path="/login" element={<WithNavbar><LoginPage /></WithNavbar>} />
            <Route path="/registro" element={<WithNavbar><RegisterPage /></WithNavbar>} />

            {/* Reservation flow (guest or authenticated) */}
            <Route path="/reservar/:id" element={
              <WithNavbar><ReservarPage /></WithNavbar>
            } />

            {/* Admin routes — Navbar + Sidebar */}
            <Route
              element={
                <ProtectedRoute allowedTypes={['admin']}>
                  <WithNavbar><MainLayout /></WithNavbar>
                </ProtectedRoute>
              }
            >
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/clientes" element={<ClientesPage />} />
              <Route path="/vehiculos" element={<VehiculosPage />} />
              <Route path="/reservas" element={<ReservasPage />} />
              <Route path="/contratos" element={<ContratosPage />} />
              <Route path="/pagos" element={<PagosPage />} />
              <Route path="/mantenimientos" element={<MantenimientosPage />} />
              <Route path="/localizaciones" element={<LocalizacionesPage />} />
              <Route path="/ubicaciones" element={<UbicacionesPage />} />
              <Route path="/extras" element={<ExtrasPage />} />
              <Route path="/usuarios" element={<UsuariosPage />} />
            </Route>

            {/* Client portal routes — Navbar + Sidebar */}
            <Route
              element={
                <ProtectedRoute allowedTypes={['cliente']}>
                  <WithNavbar><ClienteLayout /></WithNavbar>
                </ProtectedRoute>
              }
            >
              <Route path="/mi-cuenta" element={<MiCuentaPage />} />
              <Route path="/mis-reservas" element={<MisReservasPage />} />
              <Route path="/mis-contratos" element={<MisContratosPage />} />
              <Route path="/mis-facturas" element={<MisFacturasPage />} />
              <Route path="/historial" element={<HistorialPage />} />
            </Route>

            {/* Atajos legacy */}
            <Route path="/404" element={<NotFoundPage />} />

            {/* Fallback 404 */}
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </BrowserRouter>
      </QueryClientProvider>
    </ErrorBoundary>
  );
}
