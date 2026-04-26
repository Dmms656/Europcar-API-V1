import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'sonner';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import MainLayout from './components/layout/MainLayout';
import { ProtectedRoute } from './routes/ProtectedRoute';

// Public pages
import HomePage from './pages/home/HomePage';
import BuscarPage from './pages/home/BuscarPage';
import LoginPage from './pages/auth/LoginPage';

// Admin pages
import DashboardPage from './pages/dashboard/DashboardPage';
import ClientesPage from './pages/clientes/ClientesPage';
import VehiculosPage from './pages/vehiculos/VehiculosPage';
import ReservasPage from './pages/reservas/ReservasPage';
import ContratosPage from './pages/contratos/ContratosPage';
import PagosPage from './pages/pagos/PagosPage';
import MantenimientosPage from './pages/mantenimientos/MantenimientosPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: 1, refetchOnWindowFocus: false },
  },
});

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
          {/* Public routes */}
          <Route path="/" element={<HomePage />} />
          <Route path="/buscar" element={<BuscarPage />} />
          <Route path="/login" element={<LoginPage />} />

          {/* Protected admin routes */}
          <Route
            element={
              <ProtectedRoute>
                <MainLayout />
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
          </Route>

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
