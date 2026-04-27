import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'sonner';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import MainLayout from './components/layout/MainLayout';
import ClienteLayout from './components/layout/ClienteLayout';
import Navbar from './components/layout/Navbar';
import { ProtectedRoute } from './routes/ProtectedRoute';

// Public pages
import HomePage from './pages/home/HomePage';
import BuscarPage from './pages/home/BuscarPage';
import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import CatalogoPage from './pages/catalogo/CatalogoPage';
import ReservarPage from './pages/reservar/ReservarPage';

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

// Client portal pages
import MiCuentaPage from './pages/cliente/MiCuentaPage';
import MisReservasPage from './pages/cliente/MisReservasPage';
import MisContratosPage from './pages/cliente/MisContratosPage';
import MisFacturasPage from './pages/cliente/MisFacturasPage';
import HistorialPage from './pages/cliente/HistorialPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: 1, refetchOnWindowFocus: false },
  },
});

/** Wrapper that adds the shared Navbar above any page */
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
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Toaster
          position="top-right"
          toastOptions={{
            style: { background: 'var(--color-surface)', color: 'var(--color-text)', border: '1px solid var(--color-border)' },
          }}
          richColors
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

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
