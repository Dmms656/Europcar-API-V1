import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../store/useAuthStore';

export function ProtectedRoute({ children, roles }) {
  const { isAuthenticated, hasAnyRole } = useAuthStore();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (roles && roles.length > 0 && !hasAnyRole(...roles)) {
    return <Navigate to="/dashboard" replace />;
  }

  return children;
}
