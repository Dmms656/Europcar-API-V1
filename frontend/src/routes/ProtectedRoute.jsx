import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../store/useAuthStore';

export function ProtectedRoute({ children, roles, allowedTypes }) {
  const { isAuthenticated, hasAnyRole, userType } = useAuthStore();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Check user type restriction
  if (allowedTypes && allowedTypes.length > 0 && !allowedTypes.includes(userType)) {
    const redirect = userType === 'admin' ? '/dashboard' : '/mi-cuenta';
    return <Navigate to={redirect} replace />;
  }

  // Check role restriction
  if (roles && roles.length > 0 && !hasAnyRole(...roles)) {
    const redirect = userType === 'admin' ? '/dashboard' : '/mi-cuenta';
    return <Navigate to={redirect} replace />;
  }

  return children;
}
